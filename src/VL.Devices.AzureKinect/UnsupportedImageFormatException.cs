using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections.Generic;
using System.Text;

namespace VL.Devices.AzureKinect
{
    public class UnsupportedImageFormatException : Exception
    {
        public UnsupportedImageFormatException(ImageFormat format)
            : base($"The specified format {format} is not supported.")
        {
            Format = format;
        }

        public ImageFormat Format { get; }
    }
}
