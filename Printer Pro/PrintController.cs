﻿using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;

namespace PrinterPro
{
    /// <summary>
    /// 打印逻辑控制类
    /// </summary>
    class PrintController
    {
        #region 参数变量
        private const int ASCII_ZERO = 48;
        private const float Z_MAX = (float)92;
        private const int HARD_WAIT_TIME = 300;
        private float xMidRange, yMidRange;
        private static float lastX, lastY;
        private static bool initialInPosition = true;
        #endregion

        #region 数值变量
        private Console console;
        private SerialPort valve;
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="console"></param>
        /// <param name="valve"></param>
        public PrintController(Console console, SerialPort valve)
        {
            this.console = console;
            this.valve = valve;

            xMidRange = console.getXAxisMax() / 2;
            yMidRange = console.getYAxisMax() / 2;
        }

        #region 平移台运动控制

        /// <summary>
        /// 平移台弹出控制
        /// </summary>
        public void eject()
        {
            console.getXPosition(ref lastX);
            console.getYPosition(ref lastY);
            console.setXVelParams(0, 2000, (float)Data.idleSpeed);
            console.setYVelParams(0, 2000, (float)Data.idleSpeed);
            console.setZVelParams(0, 10, 20);
            console.setXAbsMovePos(xMidRange);
            console.moveXAbsolute(false);
            console.setYAbsMovePos(0);
            console.moveYAbsolute(false);
            console.setZAbsMovePos(60);
            console.moveZAbsolute(true);
        }

        // Initial decides whether start from xStart/yStart or lastX/lastY
        public void inPosition()
        {
            float xStart, yStart;
            if (initialInPosition)
            {
                xStart = (float)Data.xStart;
                yStart = (float)Data.yStart;
                initialInPosition = false;
            }
            else
            {
                xStart = lastX;
                yStart = lastY;
            }
            console.setXVelParams(0, 2000, (float)Data.idleSpeed);
            console.setYVelParams(0, 2000, (float)Data.idleSpeed);
            console.setZVelParams(0, 10, 10);
            console.setXAbsMovePos(xStart);
            console.moveXAbsolute(false);
            console.setYAbsMovePos(yStart);
            console.moveYAbsolute(false);
            console.setZAbsMovePos(Z_MAX);
            console.moveZAbsolute(true);
        }
        
        #endregion

        #region 打印控制

        /// <summary>
        /// 打印控制主入口
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="finishPrintEventHandler">结束打印回调函数</param>
        /// <param name="eventFinish1Point">打印每滴回调函数</param>
        public void beginPrint(int mode, EventHandler finishPrintEventHandler, EventHandler eventFinish1Point)
        {
            if (mode == 0) beginInkjet(finishPrintEventHandler, eventFinish1Point);
            else if (mode == 1) beginOnTheFLy(finishPrintEventHandler, eventFinish1Point);
        }

        /// <summary>
        /// 开启On the fly（持续）模式打印
        /// </summary>
        /// <param name="finishPrintEventHandler">结束打印回调函数</param>
        /// <param name="eventFinish1Point">完成1滴会地哦啊函数</param>
        private void beginOnTheFLy(EventHandler finishPrintEventHandler, EventHandler eventFinish1Point)
        {
            try
            {
                if (!valve.IsOpen)
                    valve.Open();
            }
            catch
            {
                return;
            }

            double xStart = Data.xStart, yStart = Data.yStart;
            int arraySize = Data.cols * Data.rows;
            bool direction = true;

            ArrayList printArray = new ArrayList();
            for (int j = 0; j < Data.rows; j++)
            {
                for (int k = 0; k < Data.cols; k++)
                {
                    printArray.Add(((ArrayList)((ArrayList)Data.gridData[0])[j])[k]);
                }
            }
            int[] printCount = (int[])printArray.ToArray(typeof(int));

            xStart += Data.xRelative[0];
            yStart += Data.yRelative[0];

            float accnBufferDistance = (float)(Data.workSpeed * Data.workSpeed / Data.workAccn / 2);
            float accnBufferTime = (float)(Data.workSpeed / Data.workAccn * 1000);
            for (int row = 0; row < Data.rows; row++)
            {
                if (row != 0)
                {
                    console.setYVelParams(0, Data.idleAccn, (float)Data.idleSpeed);
                    console.setYRelMoveDist(-(float)Data.yDistance);
                    console.moveYRelative(true);
                }
                // 发送数据
                byte[] buffer = new byte[4];
                buffer[0] = (byte)(ASCII_ZERO + Data.channel[0]);       // Channel ID
                buffer[1] = (byte)(ASCII_ZERO + Data.frequency[0]);     // Frequency
                buffer[2] = (byte)(ASCII_ZERO + Data.cols);             // Number of Droplets in a line
                buffer[3] = (byte)(ASCII_ZERO + Data.pulsewidth[0]);    // Pulsewidth

                if (direction)
                {
                    console.setXVelParams(0, Data.idleAccn, (float)Data.idleSpeed);
                    console.setXAbsMovePos((float)xStart - accnBufferDistance);
                    console.moveXAbsolute(true);
                    console.setXVelParams(0, Data.workAccn, (float)Data.workSpeed);
                    console.setXAbsMovePos((float)xStart + (float)Data.xDistance * Data.cols + accnBufferDistance);
                    console.moveXAbsolute(false);
                }
                else
                {
                    console.setXVelParams(0, Data.idleAccn, (float)Data.idleSpeed);
                    console.setXAbsMovePos((float)xStart + (float)Data.xDistance * Data.cols + accnBufferDistance);
                    console.moveXAbsolute(true);
                    console.setXVelParams(0, Data.workAccn, (float)Data.workSpeed);
                    console.setXAbsMovePos((float)xStart - accnBufferDistance);
                    console.moveXAbsolute(false);
                }
                // 等待加速缓冲区的时间
                Thread.Sleep((int)accnBufferTime);
                // 串口通讯，开始打印
                valve.Write(buffer, 0, 4);
                direction = !direction;

                byte[] buffer_receive = new byte[1];
                while (buffer_receive[0] != (byte)'0')
                {
                    try { valve.Read(buffer_receive, 0, 1); }
                    catch { }
                }

                for (int col = 0; col < Data.cols; col++)
                {
                    ArrayList args = new ArrayList();
                    args.Add(row);
                    args.Add(col);
                    eventFinish1Point.Invoke(args, EventArgs.Empty);
                }
                // 等待剩余时间
                // The following sleeping time saves the life of out-of-thread X Axis control. Without this compulsory
                // delay, next absolute move will probably be omitted.
                Thread.Sleep((int)(Data.xDistance * Data.cols / Data.workSpeed) * 1000 + (int)accnBufferTime + HARD_WAIT_TIME);
                Thread.Sleep(Data.waitTime);
            }
            finishPrintEventHandler.Invoke(new object(), new EventArgs());
        }

        /// <summary>
        /// 开启Inkjet（单个液滴）模式打印
        /// </summary>
        /// <param name="finishPrintEventHandler"></param>
        /// <param name="eventFinish1Point"></param>
        private void beginInkjet(EventHandler finishPrintEventHandler, EventHandler eventFinish1Point)
        {
            #region check valve port and send a message to active the port
            try
            {
                if (!valve.IsOpen)
                {
                    valve.Open();
                }
            }
            catch
            {
                return;
            }

            #endregion

            #region 变量声明
            double xStart = Data.xStart, yStart = Data.yStart;
            int arraySize = Data.cols * Data.rows;
            #endregion

            #region Inkjet printing

            if (Data.selectedStyle == 0)
            {
                #region Read print data
                ArrayList printArray = new ArrayList();
                for (int i = 0; i < Data.channelNumber; i++)
                {
                    for (int j = 0; j < Data.rows; j++)
                    {
                        for (int k = 0; k < Data.cols; k++)
                        {
                            printArray.Add(((ArrayList)((ArrayList)Data.gridData[i])[j])[k]);
                        }
                    }
                }
                int[] printCount = (int[])printArray.ToArray(typeof(int));
                #endregion

                for (int channel = 0; channel < Data.channelNumber; channel++)
                {
                    xStart += Data.xRelative[channel];
                    yStart += Data.yRelative[channel];

                    console.setXVelParams(0, 2000, (float)Data.workSpeed);
                    console.setYVelParams(0, 2000, (float)Data.workSpeed);

                    int currentXDisplay = 1;
                    int currentYDisplay = 1;

                    #region print one channel
                    for (int i = 0; i < Data.rows; i++)
                    {
                        if (yStart + i * Data.yDistance >= 0 && yStart + i * Data.yDistance <= 220)
                        {
                            console.setXAbsMovePos((float)xStart);
                            console.moveXAbsolute(true);
                            if (i != 0)
                            {
                                console.setYRelMoveDist(-(float)Data.yDistance);
                                console.moveYRelative(true);
                            }
                            for (int j = 0; j < Data.cols; j++)
                            {
                                if (xStart + j * Data.xDistance >= 0 && xStart + j * Data.xDistance <= 220)
                                {
                                    if (j != 0)
                                    {
                                        console.setXRelMoveDist((float)Data.xDistance);
                                        console.moveXRelative(true);
                                    }
                                    if (printCount[channel * arraySize + i * Data.cols + j] != 0)
                                    {
                                        byte[] buffer = new byte[4];
                                        buffer[0] = (byte)(ASCII_ZERO + Data.channel[channel]);
                                        buffer[1] = (byte)(ASCII_ZERO + Data.frequency[channel]);
                                        buffer[2] = (byte)(ASCII_ZERO + printCount[channel * arraySize + i * Data.cols + j]);
                                        buffer[3] = (byte)(ASCII_ZERO + Data.pulsewidth[channel]);
                                        valve.Write(buffer, 0, 4);

                                        // 等待电路板回复
                                        byte[] buffer_receive = new byte[1];
                                        while (buffer_receive[0] != (byte)'0')
                                        {
                                            try
                                            {
                                                valve.Read(buffer_receive, 0, 1);
                                            }
                                            catch
                                            {}
                                        }
                                        ArrayList args = new ArrayList();
                                        args.Add(i);
                                        args.Add(j);
                                        eventFinish1Point.Invoke(args, EventArgs.Empty);
                                        Thread.Sleep(Data.waitTime);
                                    }
                                    currentXDisplay++;
                                }
                                else
                                {
                                    System.Console.WriteLine("X is out of range!");
                                }
                            }
                            currentXDisplay = 1;
                            currentYDisplay++;
                        }
                        else
                        {
                            System.Console.WriteLine("Y is out of range!");
                        }
                    }
                    #endregion
                    xStart = Data.xStart;
                    yStart = Data.yStart;
                }
                finishPrintEventHandler.Invoke(new object(), new EventArgs());
            }
            #endregion
        }

        public void stop()
        {
            console.MotorX.StopImmediate(0);
            console.MotorY.StopImmediate(0);
            console.MotorZ.StopImmediate(0);
        }
        
        #endregion
    }
}
