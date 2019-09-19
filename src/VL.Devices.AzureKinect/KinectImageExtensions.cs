using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections.Generic;
using System.Text;
using VL.Lib.Basics.Imaging;

namespace VL.Devices.AzureKinect
{
    public static class KinectImageExtensions
    {
        public static IImage AsVLImage(this Image image)
        {
            var info = new ImageInfo(image.WidthPixels, image.HeightPixels, image.Format.ToPixelFormat(), image.Format.ToString());
            return new MemoryImage(image.Memory, info, image.StrideBytes);
        }

        public static PixelFormat ToPixelFormat(this ImageFormat format)
        {
            switch (format)
            {
                case ImageFormat.ColorBGRA32:
                    return PixelFormat.B8G8R8A8;
                case ImageFormat.Depth16:
                    return PixelFormat.R16;
                default:
                    throw new UnsupportedImageFormatException(format);
            }
        }
    }
}
