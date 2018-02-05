using System;
using AForge.Video;
using AForge.Video.DirectShow;

namespace PrinterPro
{
    class Camera
    {
        private AForge.Controls.VideoSourcePlayer videoSourcePlayer;
        private VideoCaptureDevice videoSource;

        public Camera(AForge.Controls.VideoSourcePlayer _videoSourcePlayer)
        {
            videoSourcePlayer = _videoSourcePlayer;
        }

        public void beginControl()
        {
            // enumerate video devices
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            // create video source
            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                OpenVideoSource(videoSource);
            }
        }

        public void openSettings()
        {
            if (videoSource != null)
            {
                videoSource.DisplayPropertyPage(IntPtr.Zero);
            }
        }

        public void endControl()
        {
            if (videoSource != null)
            {
                CloseCurrentVideoSource();
            }
        }

        // Open video source
        private void OpenVideoSource(IVideoSource source)
        {
            // stop current video source
            CloseCurrentVideoSource();

            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();
        }

        // Close video source if it is running
        private void CloseCurrentVideoSource()
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();
                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!videoSourcePlayer.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (videoSourcePlayer.IsRunning)
                {
                    videoSourcePlayer.Stop();
                }

                videoSourcePlayer.VideoSource = null;
            }
        }
    }
}
