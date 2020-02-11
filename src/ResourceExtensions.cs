using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VL.Lib.Basics.Resources;

namespace VL.Devices.AzureKinect
{
    // All this should go to VL.Core
    public static class ResourceExtensions
    {
        public static IResourceProvider<TDevice> TryOpen<TDevice>(Func<TDevice> deviceFactory, int retryCount = 3)
            where TDevice : class, IDisposable
        {
            return ResourceProvider.New(deviceFactory)
                .Retry(retryCount)
                .ShareInParallel();
        }

        /// <summary>
        /// Polls the device for data.
        /// </summary>
        /// <typeparam name="TDevice">The type of the device.</typeparam>
        /// <typeparam name="TData">The type of the data.</typeparam>
        /// <param name="device">The device to poll data from.</param>
        /// <param name="producer">The method used for polling.</param>
        /// <returns>An observable of the polled data.</returns>
        public static IObservable<TData> PollData<TDevice, TData>(this IResourceProvider<TDevice> device, Func<TDevice, TData> producer)
            where TDevice : class, IDisposable
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (producer == null)
                throw new ArgumentNullException(nameof(producer));

            return Observable.Create<TData>(async (observer, token) =>
            {
                await Task.Run(() =>
                {
                    using (var handle = device.GetHandle())
                    {
                        while (!token.IsCancellationRequested)
                        {
                            var item = producer(handle.Resource);
                            observer.OnNext(item);
                        }
                    }
                });
            }).Publish().RefCount();
        }

        public static IObservable<TData> PollData<TDevice, TData, TClock>(this IResourceProvider<TDevice> device, Func<TDevice, TClock, TData> producer, IObservable<TClock> clock)
            where TDevice : class, IDisposable
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (producer == null)
                throw new ArgumentNullException(nameof(producer));
            if (clock == null)
                throw new ArgumentNullException(nameof(clock));

            return Observable.Create<TData>(async (observer, token) =>
            {
                await Task.Run(async () =>
                {
                    using (var handle = device.GetHandle())
                    {
                        clock.Subscribe(c =>
                        {
                            var item = producer(handle.Resource, c);
                            observer.OnNext(item);
                        }, token);
                        await token.WhenCanceled();
                    }
                });
            }).Publish().RefCount();
        }

        public static IObservable<TResource> PollResource<TDevice, TResource>(this IResourceProvider<TDevice> device, Func<TDevice, TResource> producer)
            where TDevice : class, IDisposable
            where TResource : IDisposable
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (producer == null)
                throw new ArgumentNullException(nameof(producer));

            return Observable.Create<TResource>(async (observer, token) =>
            {
                await Task.Run(() =>
                {
                    using (var handle = device.GetHandle())
                    {
                        while (!token.IsCancellationRequested)
                        {
                            using (var resource = producer(handle.Resource))
                            {
                                if (resource != null)
                                    observer.OnNext(resource);
                                else
                                    break;
                            }
                        }
                    }
                });
            }).Publish().RefCount();
        }

        public static IObservable<TResource> PollResourceAndYield<TDevice, TResource, TState>(this IResourceProvider<TDevice> device, Func<TState> create, Func<TState, TDevice, Tuple<TState, TResource>> update)
            where TDevice : class, IDisposable
            where TResource : IDisposable
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (update == null)
                throw new ArgumentNullException(nameof(update));

            return Observable.Create<TResource>(async (observer, token) =>
            {
                await Task.Run(async () =>
                {
                    using (var handle = device.GetHandle())
                    {
                        var state = create();
                        while (!token.IsCancellationRequested)
                        {
                            var (s, resource) = update(state, handle.Resource);
                            state = s;
                            using (resource)
                            {
                                if (resource != null)
                                    observer.OnNext(resource);
                                else
                                    await Task.Delay(1).ConfigureAwait(false);
                            }
                        }

                        if (state is IDisposable disposable)
                            disposable.Dispose();
                    }
                });
            }).Publish().RefCount();
        }

        public static IObservable<TResource> PollResource<TDevice, TResource, TClock>(this IResourceProvider<TDevice> device, Func<TDevice, TClock, TResource> producer, IObservable<TClock> clock)
            where TDevice : class, IDisposable
            where TResource : IDisposable
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (producer == null)
                throw new ArgumentNullException(nameof(producer));
            if (clock == null)
                throw new ArgumentNullException(nameof(clock));

            return Observable.Create<TResource>(async (observer, token) =>
            {
                await Task.Run(async () =>
                {
                    using (var handle = device.GetHandle())
                    {
                        var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                        clock.Subscribe(c =>
                        {
                            using (var resource = producer(handle.Resource, c))
                            {
                                if (resource != null)
                                    observer.OnNext(resource);
                                else
                                    cts.Cancel();
                            }
                        }, cts.Token);
                        await cts.Token.WhenCanceled();
                    }
                });
            }).Publish().RefCount();
        }

        static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        public static IResourceProvider<T> Retry<T>(this IResourceProvider<T> provider, int retryCount = 3)
            where T : class, IDisposable
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            return new Provider<T>(() =>
            {
                var attempts = 0;
                Beginning:
                try
                {
                    return provider.GetHandle();
                }
                catch (Exception)
                {
                    if (attempts++ < retryCount)
                    {
                        Thread.Sleep(500);
                        goto Beginning;
                    }
                    else
                    {
                        throw;
                    }
                }
            });
        }

        class Provider<TResource> : IResourceProvider<TResource>
            where TResource : class, IDisposable
        {
            readonly Func<IResourceHandle<TResource>> FProducer;

            public Provider(Func<IResourceHandle<TResource>> producer)
            {
                FProducer = producer;
            }
            public IResourceHandle<TResource> GetHandle() => FProducer();
        }
    }
}
