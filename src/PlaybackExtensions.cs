using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.Record;
using System;

namespace VL.Devices.AzureKinect
{
    public static class PlaybackExtensions
    {
        public static long GetFrameRate(this Playback playback)
        {
            var config = playback.RecordConfiguration;
            switch (config.CameraFPS)
            {
                case FPS.FPS5:
                    return 5;
                case FPS.FPS15:
                    return 15;
                case FPS.FPS30:
                    return 30;
                default:
                    throw new NotImplementedException();
            }

            // Causes AV
            //for (int i = 0; i < playback.TrackCount; i++)
            //{
            //    var trackName = playback.GetTrackName(i);
            //    var settings = playback.GetTrackVideoSettings(trackName);
            //    return settings.FrameRate;
            //}
            //return 0;
        }
    }
}
