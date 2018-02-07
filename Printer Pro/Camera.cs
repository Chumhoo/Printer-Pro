using System;
using AForge.Video;
using AForge.Video.DirectShow;

namespace PrinterPro
{
    /// <summary>
    /// 相机控制类
    /// </summary>
    class Camera
    {
        #region 私有对象变量
        private AForge.Controls.VideoSourcePlayer videoSourcePlayer;
        private VideoCaptureDevice videoSource;
        #endregion

        public Camera(AForge.Controls.VideoSourcePlayer _videoSourcePlayer)
        {
            // 将对象变量实例化
            videoSourcePlayer = _videoSourcePlayer;
        }

        // 开启摄像头控制
        public void beginControl()
        {
            // 遍历视频设备
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            // 创建视频源
            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                OpenVideoSource(videoSource);
            }
        }

        // 开启设置窗口
        public void openSettings()
        {
            if (videoSource != null)
            {
                videoSource.DisplayPropertyPage(IntPtr.Zero);
            }
        }

        // 结束控制
        public void endControl()
        {
            if (videoSource != null)
            {
                CloseCurrentVideoSource();
            }
        }

        // 打开视频源
        private void OpenVideoSource(IVideoSource source)
        {
            // 终止当前视频源
            CloseCurrentVideoSource();
            // 开始新的视频源
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();
        }

        // 关闭正在运行中的视频源
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
