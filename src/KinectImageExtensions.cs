using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Reactive.Linq;
using VL.Lib.Basics.Imaging;
using VL.Lib.Collections;
using Stride.Core.Mathematics;

namespace VL.Devices.AzureKinect
{
    public static class KinectImageExtensions
    {
        struct XYZ
        {
            public short x;
            public short y;
            public short z;
        }

        /// <summary>
        /// Tries to decompress the image. Will return true if the image needed to be decompressed and therefor should get disposed of by the caller.
        /// </summary>
        /// <param name="image">The image which might be in a compressed form.</param>
        /// <param name="decompressedImage">The decompressed image if decrompression was necessary or the original image.</param>
        /// <returns>True if decompression was neccessary.</returns>
        public static bool TryToDecompress(this Image image, out Image decompressedImage)
        {
            switch (image.Format)
            {
                case ImageFormat.ColorMJPG:
                    decompressedImage = image.Decompress();
                    return true;
                default:
                    decompressedImage = image;
                    return false;
            }
        }

        /// <summary>
        /// Selects the color images out of the captures. Will also decompress them if needed.
        /// </summary>
        /// <param name="captures">The captures to select the color images from.</param>
        /// <returns>The color images of the captures.</returns>
        public static IObservable<IImage> SelectColorImages(this IObservable<Capture> captures)
        {
            return captures.SelectMany(c =>
            {
                var image = c.Color; 
                if (image != null)
                {
                    if (image.TryToDecompress(out var decompressedImage))
                        return Observable.Using(() => decompressedImage, i => Observable.Return(i.AsVLImage()));
                    else
                        return Observable.Return(image.AsVLImage());
                }
                return Observable.Empty<IImage>();
            });
        }

        /// <summary>
        /// Selects the depth images out of the captures.
        /// </summary>
        /// <param name="captures">The captures to select the depth images from.</param>
        /// <returns>The depth images of the captures.</returns>
        public static IObservable<IImage> SelectDepthImages(this IObservable<Capture> captures)
        {
            return captures.SelectMany(c =>
            {
                var image = c.Depth;
                if (image != null)
                {
                    return Observable.Return(image.AsVLImage());
                }
                return Observable.Empty<IImage>();
            });
        }

        /// <summary>
        /// Selects the infrared images out of the captures.
        /// </summary>
        /// <param name="captures">The captures to select the infrared images from.</param>
        /// <returns>The infrared images of the captures.</returns>
        public static IObservable<IImage> SelectInfraredImages(this IObservable<Capture> captures)
        {
            return captures.SelectMany(c =>
            {
                var image = c.IR;
                if (image != null)
                {
                    return Observable.Return(image.AsVLImage());
                }
                return Observable.Empty<IImage>();
            });
        }

        /// <summary>
        /// Selects the timestamps out of the captures.
        /// </summary>
        /// <param name="captures">The captures to select the timestamps from.</param>
        /// <returns>The timestamps of the captures.</returns>
        public static IObservable<TimeSpan> SelectTimestamp(this IObservable<Capture> captures)
        {
            return captures.SelectMany(c =>
            {
                return Observable.Return(c.GetTimestamp());
            });
        }

        /// <summary>
        /// Since a capture can miss either the color or depth image this operator will take care of always producing
        /// a capture where the latest received color and depth images are set.
        /// </summary>
        public static IObservable<Capture> CombineLatestImages(this IObservable<Capture> captures)
        {
            var latestColor = default(Image);
            var latestDepth = default(Image);
            var latestIR = default(Image);
            return captures.SelectMany(c =>
            {
                if (c.Color != null)
                {
                    latestColor?.Dispose();
                    latestColor = c.Color.Reference();
                }
                if (c.Depth != null)
                {
                    latestDepth?.Dispose();
                    latestDepth = c.Depth.Reference();
                }
                if (c.IR != null)
                {
                    latestIR?.Dispose();
                    latestIR = c.IR.Reference();
                }
                if (latestColor != null || latestDepth != null || latestIR != null)
                {
                    return Observable.Using(
                        () => CreateCapture(latestColor?.Reference(), latestDepth?.Reference(), latestIR?.Reference(), c.Temperature),
                        x => Observable.Return(x));
                }
                return Observable.Empty<Capture>();
            });
        }

        public static IObservable<Capture> DecompressColorImages(this IObservable<Capture> captures)
        {
            return captures.SelectMany(c =>
            {
                var colorImage = c.Color;
                if (colorImage != null && colorImage.TryToDecompress(out var decompressedImage))
                    return Observable.Using(
                        () => CreateCapture(decompressedImage, c.Depth?.Reference(), c.IR?.Reference(), c.Temperature), 
                        x => Observable.Return(x));
                else
                    return Observable.Return(c);
            });
        }

        private static Capture CreateCapture(Image color, Image depth, Image ir, float temperature)
        {
            var capture = new Capture() { Temperature = temperature };
            // The setters crash when assigning null
            if (color != null)
                capture.Color = color;
            if (depth != null)
                capture.Depth = depth;
            if (ir != null)
                capture.IR = ir;
            return capture;
        }

        public static IImage AsVLImage(this Image image)
        {
            ReadOnlyMemory<byte> memory = image.Memory;
            return memory.ToImage(image.WidthPixels, image.HeightPixels, image.Format.ToPixelFormat(), image.Format.ToString(), isVolatile: true);
        }

        public static Spread<Vector3> GetPointCloudData(this Image image, float scaling = 1f)
        {
            if (image.Format != ImageFormat.Custom)
                throw new UnsupportedImageFormatException(image.Format);

            var data = image.GetPixels<XYZ>();
            var result = new SpreadBuilder<Vector3>(data.Length);
            foreach (var pixel in data.Span)
            {
                result.Add(new Vector3(pixel.x * scaling, pixel.y * scaling, pixel.z * scaling));
            }
            return result.ToSpread();
        }

        public static IImage PointCloudeImageToBGRAImage(this Image image, float scaling = 1f)
        {
            if (image.Format != ImageFormat.Custom)
                throw new UnsupportedImageFormatException(image.Format);

            var data = image.GetPixels<XYZ>();
            var pixels = new ColorBGRA[data.Length];
            var i = 0;
            foreach (var pixel in data.Span)
            {
                pixels[i++] = new ColorBGRA(pixel.x * scaling, pixel.y * scaling, pixel.z * scaling, 1);
            }
            return pixels.ToImage(image.WidthPixels, image.HeightPixels, PixelFormat.B8G8R8A8);
        }

        public static PixelFormat ToPixelFormat(this ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.ColorBGRA32:
                    return PixelFormat.B8G8R8A8;
                case ImageFormat.Depth16:
                    return PixelFormat.R16;
                case ImageFormat.IR16:
                    return PixelFormat.R16;
                case ImageFormat.Custom16:
                    return PixelFormat.R16;
                case ImageFormat.Custom8:
                    return PixelFormat.R8;
                default:
                    throw new UnsupportedImageFormatException(format);
            }
        }
    }
}
