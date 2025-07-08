using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Runtime.InteropServices;
using ImageFormat = Microsoft.Azure.Kinect.Sensor.ImageFormat;
using KinectImage = Microsoft.Azure.Kinect.Sensor.Image;

namespace VL.Devices.AzureKinect
{
    static class ImageDecompression
    {
        static readonly IImageDecoder jpgDecoder = new JpegDecoder() { IgnoreMetadata = true };

        public static KinectImage Decompress(this KinectImage image)
        {
            if (image.Format != ImageFormat.ColorMJPG)
                throw new ArgumentException("Expected MJPG format", nameof(image));

            using (var decodedImage = Image.Load<Bgra32>(image.Memory.Span, jpgDecoder))
            {
                var kinectImage = new KinectImage(ImageFormat.ColorBGRA32, decodedImage.Width, decodedImage.Height)
                {
                    DeviceTimestamp = image.DeviceTimestamp,
                    Exposure = image.Exposure,
                    ISOSpeed = image.ISOSpeed,
                    SystemTimestampNsec = image.SystemTimestampNsec,
                    WhiteBalance = image.WhiteBalance
                };
                decodedImage.CopyPixelDataTo(kinectImage.Memory.Span);
                return kinectImage;
            }
        }
    }
}
