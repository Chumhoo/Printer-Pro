using MetroFramework.Forms;
using System;
using System.Threading;

namespace PrinterPro
{
    public partial class Console : MetroForm
    {
        private bool XHomeReady = false, YHomeReady = false, ZHomeReady = false;
        private EventHandler homeCompleteHandler;
        private float xMax = 0, yMax = 0;
        private bool XEnable, YEnable, ZEnable;

        public Console(bool _XEnable, bool _YEnable, bool _ZEnable)
        {
            InitializeComponent();
            XEnable = _XEnable;
            YEnable = _YEnable;
            ZEnable = _ZEnable;
        }

        public void setXEnabled(bool enabled) { XEnable = enabled; }
        public void setYEnabled(bool enabled) { YEnable = enabled; }
        public void setZEnabled(bool enabled) { ZEnable = enabled; }
        public float getXAxisMax() { if (!XEnable) return 0; else return xMax; }
        public float getYAxisMax() { if (!YEnable) return 0; else return yMax; }
        public float getZAxisMax() { if (!ZEnable) return 0; else return MotorZ.GetStageAxisInfo_MaxPos(0); }
        public void getXPosition(ref float x) { if (!XEnable) x = 0; else MotorX.GetPosition(0, ref x); }
        public void getYPosition(ref float y) { if (!YEnable) y = 0; else MotorY.GetPosition(0, ref y); }
        public void getZPosition(ref float z) { if (!ZEnable) z = 0; else MotorZ.GetPosition(0, ref z); }
        public void setXVelParams(float minVel, float accn, float maxVel) { if (XEnable) MotorX.SetVelParams(0, minVel, accn, maxVel); }
        public void setYVelParams(float minVel, float accn, float maxVel) { if (YEnable) MotorY.SetVelParams(0, minVel, accn, maxVel); }
        public void setZVelParams(float minVel, float accn, float maxVel) { if (ZEnable) MotorZ.SetVelParams(0, minVel, accn, maxVel); }
        public void setXAbsMovePos(float absPos) { if (XEnable) MotorX.SetAbsMovePos(0, absPos); }
        public void setYAbsMovePos(float absPos) { if (YEnable) MotorY.SetAbsMovePos(0, absPos); }
        public void setZAbsMovePos(float absPos) { if (ZEnable) MotorZ.SetAbsMovePos(0, absPos); }
        public void setXRelMoveDist(float relPos) { if (XEnable) MotorX.SetRelMoveDist(0, relPos); }
        public void setYRelMoveDist(float relPos) { if (YEnable) MotorY.SetRelMoveDist(0, relPos); }
        public void setZRelMoveDist(float relPos) { if (ZEnable) MotorZ.SetRelMoveDist(0, relPos); }
        public void moveXAbsolute(bool wait) { if (XEnable) MotorX.MoveAbsolute(0, wait); }
        public void moveYAbsolute(bool wait) { if (YEnable) MotorY.MoveAbsolute(0, wait); }
        public void moveZAbsolute(bool wait) { if (ZEnable) MotorZ.MoveAbsolute(0, wait); }
        public void moveXRelative(bool wait) { if (XEnable) MotorX.MoveRelative(0, wait); }
        public void moveYRelative(bool wait) { if (YEnable) MotorY.MoveRelative(0, wait); }
        public void moveZRelative(bool wait) { if (ZEnable) MotorZ.MoveRelative(0, wait); }


        public bool StartCtrl(bool autoHome, EventHandler _homeCompleteHandler)
        {
            try
            {
                MotorX.StartCtrl();
                MotorY.StartCtrl();
                MotorZ.StartCtrl();

                xMax = MotorX.GetStageAxisInfo_MaxPos(0);
                yMax = MotorY.GetStageAxisInfo_MaxPos(0);

                // MotorX.EnableEventDlg(false);

                if (autoHome)
                {
                    Thread.Sleep(100);
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

        private int plDirection = 0, plLimSwitch = 0;
        private float pfHomeVel = 0, pfZeroOffset = 0;

        public void moveXHome()
        {
            MotorX.HomeComplete += MotorX_HomeComplete;
            MotorX.GetHomeParams(0, ref plDirection, ref plLimSwitch, ref pfHomeVel, ref pfZeroOffset);
            MotorX.SetHomeParams(0, 1, 1, 50, 0);
            MotorX.MoveHome(0, false);
        }

        public void moveYHome()
        {
            MotorY.HomeComplete += MotorY_HomeComplete;
            MotorY.GetHomeParams(0, ref plDirection, ref plLimSwitch, ref pfHomeVel, ref pfZeroOffset);
            MotorY.SetHomeParams(0, 1, 1, 50, 0);
            MotorY.MoveHome(0, false);
        }

        public void moveZHome()
        {
            const int DANGER_PARAMETER = 2;
            MotorZ.HomeComplete += MotorZ_HomeComplete;
            setZVelParams(0, 10, 20);
            setZAbsMovePos(0);
            moveZAbsolute(true);
            // The DANGER_PARAMETER decides which direction does Z-axis move towards. 2 means lifting while 1 means sinking.
            // Please be careful when changing it for god's sake!
            MotorZ.SetHomeParams(0, DANGER_PARAMETER, 1, 5, 0);
            MotorZ.MoveHome(0, false);
        }

        public void MoveHome(EventHandler handler)
        {
            homeCompleteHandler = handler;

            if (XEnable) moveXHome();
            if (YEnable) moveYHome();
            if (ZEnable) moveZHome();
        }

        private void MotorZ_HomeComplete(object sender, AxMG17MotorLib._DMG17MotorEvents_HomeCompleteEvent e)
        {
            ZHomeReady = true;
            if ((XHomeReady || !XEnable) && (YHomeReady || !YEnable) && (ZHomeReady || !ZEnable))
            {
                homeCompleteHandler.Invoke(sender, new EventArgs());
            }
        }

        private void MotorY_HomeComplete(object sender, AxMG17MotorLib._DMG17MotorEvents_HomeCompleteEvent e)
        {
            YHomeReady = true;
            if ((XHomeReady || !XEnable) && (YHomeReady || !YEnable) && (ZHomeReady || !ZEnable))
            {
                homeCompleteHandler.Invoke(sender, new EventArgs());
            }
        }

        private void MotorX_HomeComplete(object sender, AxMG17MotorLib._DMG17MotorEvents_HomeCompleteEvent e)
        {
            XHomeReady = true;
            if ((XHomeReady || !XEnable) && (YHomeReady || !YEnable) && (ZHomeReady || !ZEnable))
            {
                homeCompleteHandler.Invoke(sender, new EventArgs());
            }
        }
    }
}
