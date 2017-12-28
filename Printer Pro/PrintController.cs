using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;

namespace PrinterPro
{
    class PrintController
    {
        private const int ASCII_ZERO = 48;
        private Console console;
        private SerialPort valve;

        private float xMidRange, yMidRange;
        private static float lastX, lastY;

        public PrintController(Console console, SerialPort valve)
        {
            this.console = console;
            this.valve = valve;

            xMidRange = console.MotorX.GetStageAxisInfo_MaxPos(0) / 2;
            yMidRange = console.MotorY.GetStageAxisInfo_MaxPos(0) / 2;
        }

        public void eject()
        {
            console.MotorX.GetPosition(0, ref lastX);
            console.MotorY.GetPosition(0, ref lastY);
            console.MotorX.SetVelParams(0, 0, 2000, (float)Data.idleSpeed);
            console.MotorY.SetVelParams(0, 0, 2000, (float)Data.idleSpeed);
            console.MotorX.SetAbsMovePos(0, xMidRange);
            console.MotorX.MoveAbsolute(0, false);
            console.MotorY.SetAbsMovePos(0, 0);
            console.MotorY.MoveAbsolute(0, true);
        }

        // Initial decides whether start from xStart/yStart or lastX/lastY
        public void inPosition(bool initial)
        {
            float xStart, yStart;
            if (initial)
            {
                xStart = (float)Data.xStart;
                yStart = (float)Data.yStart;
            }
            else
            {
                xStart = lastX;
                yStart = lastY;
            }
            console.MotorX.SetVelParams(0, 0, 2000, (float)Data.idleSpeed);
            console.MotorY.SetVelParams(0, 0, 2000, (float)Data.idleSpeed);
            console.MotorX.SetAbsMovePos(0, xStart);
            console.MotorX.MoveAbsolute(0, false);
            console.MotorY.SetAbsMovePos(0, yStart);
            console.MotorY.MoveAbsolute(0, true);
        }

        public void beginPrint(EventHandler handler)
        {
            #region check valve port and send a message to active the port
            try
            {
                valve.Open();
                valve.Close();
            }
            catch
            {
                return;
            }

            #endregion

            double xStart = Data.xStart, yStart = Data.yStart;
            int arraySize = Data.colCount * Data.rowCount;
            #region Inkjet printing

            if (Data.selectedStyle == 0)
            {
                #region Read print data
                ArrayList printArray = new ArrayList();
                for (int i = 0; i < Data.SolutionNumber; i++)
                {
                    for (int j = 0; j < Data.rowCount; j++)
                    {
                        for (int k = 0; k < Data.colCount; k++)
                        {
                            printArray.Add(((ArrayList)((ArrayList)Data.gridData[i])[j])[k]);
                        }
                    }
                }
                int[] printCount = (int[])printArray.ToArray(typeof(int));
                #endregion

                for (int channel = 0; channel < Data.SolutionNumber; channel++)
                {
                    xStart += Data.xRelative[channel];
                    yStart += Data.yRelative[channel];

                    console.MotorX.SetVelParams(0, 0, 2000, (float)Data.workSpeed);
                    console.MotorY.SetVelParams(0, 0, 2000, (float)Data.workSpeed);

                    int currentXDisplay = 1;
                    int currentYDisplay = 1;

                    #region print one channel
                    valve.Open();
                    for (int i = 0; i < Data.rowCount; i++)
                    {
                        if (yStart + i * Data.yDistance >= 0 && yStart + i * Data.yDistance <= 220)
                        {
                            console.MotorX.SetAbsMovePos(0, (float)xStart);
                            console.MotorX.MoveAbsolute(0, true);
                            if (i != 0)
                            {
                                console.MotorY.SetRelMoveDist(0, -(float)Data.yDistance);
                                console.MotorY.MoveRelative(0, true);
                            }
                            for (int j = 0; j < Data.colCount; j++)
                            {
                                if (xStart + j * Data.xDistance >= 0 && xStart + j * Data.xDistance <= 220)
                                {
                                    if (j != 0)
                                    {
                                        console.MotorX.SetRelMoveDist(0, (float)Data.xDistance);
                                        console.MotorX.MoveRelative(0, true);
                                    }
                                    if (printCount[channel * arraySize + i * Data.colCount + j] != 0)
                                    {
                                        byte[] buffer = new byte[4];
                                        buffer[0] = (byte)(ASCII_ZERO + Data.channel[channel]);
                                        buffer[1] = (byte)(ASCII_ZERO + Data.frequency[channel]);
                                        buffer[2] = (byte)(ASCII_ZERO + printCount[channel * arraySize + i * Data.colCount + j]);
                                        buffer[3] = (byte)(ASCII_ZERO + Data.pulsewidth[channel]);
                                        valve.Write(buffer, 0, 4);

                                        byte[] buffer_receive = new byte[1];
                                        while (buffer_receive[0] != (byte)'0')
                                        {
                                            try
                                            {
                                                valve.Read(buffer_receive, 0, 1);
                                            }
                                            catch
                                            {

                                            }
                                        }
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
                    valve.Close();
                    #endregion
                    xStart = Data.xStart;
                    yStart = Data.yStart;
                }
                handler.Invoke(new object(), new EventArgs());
            }
            #endregion
        }

        public void stop()
        {
            console.MotorX.StopImmediate(0);
            console.MotorY.StopImmediate(0);
            if (valve.IsOpen)
            {
                valve.Close();
            }
        }
    }
}
