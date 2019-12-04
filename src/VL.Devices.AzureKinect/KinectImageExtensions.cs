using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using VL.Lib.Basics.Imaging;
using VL.Lib.Collections;
using Xenko.Core.Mathematics;

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

        public static IImage AsVLImage(this Image image)
        {
            var info = new ImageInfo(image.WidthPixels, image.HeightPixels, image.Format.ToPixelFormat(), image.Format.ToString());
            return new MemoryImage<byte>(image.Memory, info, image.StrideBytes);
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
            return new MemoryImage<ColorBGRA>(pixels.AsMemory(), new ImageInfo(image.WidthPixels, image.HeightPixels, PixelFormat.B8G8R8A8), image.WidthPixels * 4);
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
