using MetroFramework.Forms;
using System;
using System.Threading;

namespace PrinterPro
{
    public partial class Console : MetroForm
    {
        private bool XHomeReady = false, YHomeReady = false;
        private EventHandler homeCompleteHandler;

        public Console()
        {
            InitializeComponent();
        }

        public bool StartCtrl(bool autoHome, EventHandler _homeCompleteHandler)
        {
            try
            {
                MotorX.StartCtrl();
                MotorY.StartCtrl();
                MotorZ.StartCtrl();
                if (autoHome)
                {
                    Thread.Sleep(50);
                    MoveHome(_homeCompleteHandler);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void EndCtrl()
        {
            try
            {
                MotorX.StopCtrl();
                MotorY.StopCtrl();
                MotorZ.StopCtrl();
            }
            catch
            {}
        }

        public void MoveHome(EventHandler handler)
        {
            int plDirection = 0, plLimSwitch = 0;
            float pfHomeVel = 0, pfZeroOffset = 0;
            MotorX.GetHomeParams(0, ref plDirection, ref plLimSwitch, ref pfHomeVel, ref pfZeroOffset);
            MotorY.GetHomeParams(0, ref plDirection, ref plLimSwitch, ref pfHomeVel, ref pfZeroOffset);
            MotorX.SetHomeParams(0, 1, 1, 50, 0);
            MotorY.SetHomeParams(0, 1, 1, 50, 0);
            MotorX.MoveHome(0, false);
            MotorY.MoveHome(0, false);
            MotorX.HomeComplete += MotorX_HomeComplete;
            MotorY.HomeComplete += MotorY_HomeComplete;
            homeCompleteHandler = handler;
        }

        private void MotorY_HomeComplete(object sender, AxMG17MotorLib._DMG17MotorEvents_HomeCompleteEvent e)
        {
            XHomeReady = true;
            if (XHomeReady && YHomeReady)
            {
                homeCompleteHandler.Invoke(sender, new EventArgs());
            }
        }

        private void MotorX_HomeComplete(object sender, AxMG17MotorLib._DMG17MotorEvents_HomeCompleteEvent e)
        {
            YHomeReady = true;
            if (XHomeReady && YHomeReady)
            {
                homeCompleteHandler.Invoke(sender, new EventArgs());
            }
        }
    }
}
