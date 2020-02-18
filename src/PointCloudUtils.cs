using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Kinect.Sensor;
using VL.Lib.Collections;
using System.Numerics;
using System.Linq;

namespace VL.Devices.AzureKinect
{
    public static class PointCloudUtils
    {
        class ExtrinsicsComparer : IEqualityComparer<Extrinsics>
        {
            public static readonly ExtrinsicsComparer Default = new ExtrinsicsComparer();

            public bool Equals(Extrinsics x, Extrinsics y)
            {
                return x.Rotation.SequenceEqual(y.Rotation) &&
                       x.Translation.SequenceEqual(y.Translation);
            }

            public int GetHashCode(Extrinsics obj)
            {
                return 0;
            }
        }

        class IntrinsicsComparer : IEqualityComparer<Intrinsics>
        {
            public static readonly IntrinsicsComparer Default = new IntrinsicsComparer();

            public bool Equals(Intrinsics x, Intrinsics y)
            {
                return x.Type == y.Type &&
                       x.ParameterCount == y.ParameterCount &&
                       // The last 4 values are very very small so let's ignore them as it would cause equality check to fail
                       x.Parameters.Take(12).SequenceEqual(y.Parameters.Take(12));
            }

            public int GetHashCode(Intrinsics obj)
            {
                return 0;
            }
        }

        class CameraCalibrationComparer : IEqualityComparer<CameraCalibration>
        {
            public static readonly CameraCalibrationComparer Default = new CameraCalibrationComparer();

            public bool Equals(CameraCalibration x, CameraCalibration y)
            {
                return x.ResolutionWidth == y.ResolutionWidth &&
                       x.ResolutionHeight == y.ResolutionHeight &&
                       x.MetricRadius == y.MetricRadius &&
                       ExtrinsicsComparer.Default.Equals(x.Extrinsics, y.Extrinsics) &&
                       IntrinsicsComparer.Default.Equals(x.Intrinsics, y.Intrinsics);

            }

            public int GetHashCode(CameraCalibration obj)
            {
                return 0;
            }
        }
        class CalibrationComparer : IEqualityComparer<Calibration>
        {
            public static readonly CalibrationComparer Default = new CalibrationComparer();

            public bool Equals(Calibration x, Calibration y)
            {
                return x.ColorResolution == y.ColorResolution &&
                       x.DepthMode == y.DepthMode &&
                       CameraCalibrationComparer.Default.Equals(x.ColorCameraCalibration, y.ColorCameraCalibration) &&
                       CameraCalibrationComparer.Default.Equals(x.DepthCameraCalibration, y.DepthCameraCalibration) &&
                       x.DeviceExtrinsics.SequenceEqual(y.DeviceExtrinsics, ExtrinsicsComparer.Default);
            }

            public int GetHashCode(Calibration obj)
            {
                return obj.ColorResolution.GetHashCode();
            }
        }

        static Dictionary<Calibration, Spread<Xenko.Core.Mathematics.Vector2>> depthRayTableCache = 
            new Dictionary<Calibration, Spread<Xenko.Core.Mathematics.Vector2>>(CalibrationComparer.Default);
        public static Spread<Xenko.Core.Mathematics.Vector2> BuildDepthRayTable(Calibration calibration)
        {
            if (depthRayTableCache.TryGetValue(calibration, out var rayTable))
                return rayTable;

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

            rayTable = table_data.ToSpread();
            depthRayTableCache[calibration] = rayTable;
            return rayTable;
        }

        static Dictionary<Calibration, Spread<Xenko.Core.Mathematics.Vector2>> colorRayTableCache =
            new Dictionary<Calibration, Spread<Xenko.Core.Mathematics.Vector2>>(CalibrationComparer.Default);
        public static Spread<Xenko.Core.Mathematics.Vector2> BuildColorRayTable(Calibration calibration)
        {
            if (colorRayTableCache.TryGetValue(calibration, out var rayTable))
                return rayTable;

            int width = calibration.ColorCameraCalibration.ResolutionWidth;
            int height = calibration.ColorCameraCalibration.ResolutionHeight;

            var table_data = new Xenko.Core.Mathematics.Vector2[width * height];// xy_table. (k4a_float2_t *)(void*)k4a_image_get_buffer(xy_table);

            Vector2 p;
            for (int y = 0, idx = 0; y < height; y++)
            {
                p.Y = y;
                for (int x = 0; x < width; x++, idx++)
                {
                    p.X = x;
                    var ray = calibration.TransformTo3D(p, 1, CalibrationDeviceType.Color, CalibrationDeviceType.Color);

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

            rayTable = table_data.ToSpread();
            colorRayTableCache[calibration] = rayTable;
            return rayTable;
        }

        static Dictionary<Calibration, Transformation> transformationCache =
            new Dictionary<Calibration, Transformation>(CalibrationComparer.Default);

        public static Transformation GetCachedTransformation(Calibration calibration)
        {
            if (!transformationCache.TryGetValue(calibration, out var transformation))
                transformationCache[calibration] = transformation = new Transformation(calibration);
            return transformation;
        }
    }
}
