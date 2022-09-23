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
    public static class ResourceExtensions
    {
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
    }
}
