using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Kinect.Sensor;
using VL.Lib.Collections;
using System.Numerics;

namespace VL.Devices.AzureKinect
{
    public static class PointCloudUtils
    {
        public static Spread<Xenko.Core.Mathematics.Vector2> BuildRayTable(Calibration calibration)
        {

            int width = calibration.DepthCameraCalibration.ResolutionWidth;
            int height = calibration.DepthCameraCalibration.ResolutionHeight;

            var table_data = new Xenko.Core.Mathematics.Vector2[width * height];// xy_table. (k4a_float2_t *)(void*)k4a_image_get_buffer(xy_table);

            Vector2 p;
            for (int y = 0, idx = 0; y < height; y++)
            {
                p.Y = y;
                for (int x = 0; x < width; x++, idx++)
                {
                    p.X = x;
                    var ray = calibration.TransformTo3D(p, 1, CalibrationDeviceType.Depth, CalibrationDeviceType.Depth);

                    if (ray.HasValue)
                    {
                        table_data[idx].X = ray.Value.X;
                        table_data[idx].Y = -ray.Value.Y;
                    }
                    else
                    {
                        table_data[idx].X = float.NaN;
                        table_data[idx].Y = float.NaN;
                    }
                }
            }

            return table_data.ToSpread();
        }
    }
}
