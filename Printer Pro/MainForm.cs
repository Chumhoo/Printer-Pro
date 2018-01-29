﻿using System;
using System.Data;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PrinterPro
{
    public partial class MainForm : MetroForm
    {
        #region variables

        private enum PRINT_STATE { PRINTING, SUSPENDED, STOPPED };
        private enum EJECT_STATE { INITIAL, EJECTED, INPOSITION };
        private enum CYCLE_STAGES { PRE, S1, S2, EXT };
        private PRINT_STATE printState = PRINT_STATE.STOPPED;
        private EJECT_STATE ejectState = EJECT_STATE.INITIAL;

        private const int ASCII_ZERO = 48;
        public ManualResetEvent pauseEvent = new ManualResetEvent(true);
        public ManualResetEvent stopEvent = new ManualResetEvent(true);
        public SerialPort spAirValve = new SerialPort();
        public SerialPort spHeater = new SerialPort();
        private Console console;
        private DataGrid dataGrid = new DataGrid();
        private FileLoader fileLoader = new FileLoader();
        private PrintController printController;
        private PictureBox pbTarget = new PictureBox();
        private Graphics graphicsXYStage, graphicsFrequency;
        private Stopwatch stopwatch = new Stopwatch();

        float savedX = 0, savedY = 0;
        long timeUsed = 0;

        private bool homeReady = false, fileReady = false, consoleReady = false;

        private Thread threadPrint = null, threadConsole = null, threadFile = null;
        private Thread threadPositionMonitor = null, threadTemperatureMonitor = null;
        private Thread threadTemperatureController = null;

        private DateTime initialTime = DateTime.Now;
        private double temperature = 0;
        #endregion


        public MainForm()
        {
            InitializeComponent();

            tabControl.SelectedIndex = 0;
            cbFromCurrent.Visible = false;
            btnOpenConsole.Enabled = false;
            btnPrint.Enabled = false;
            btnEject.Enabled = false;

            graphicsXYStage = panelXYStage.CreateGraphics();
            graphicsFrequency = panelFrequency.CreateGraphics();
            
            cbPrintMode.SelectedIndex = Properties.Settings.Default.PrintMode;
            cbChannel.SelectedIndex = 0;

            togXAxis.Checked = PrinterPro.Properties.Settings.Default.X_Enable;
            togYAxis.Checked = PrinterPro.Properties.Settings.Default.Y_Enable;
            togZAxis.Checked = PrinterPro.Properties.Settings.Default.Z_Enable;
        }

        private void refreshValvePort()
        {
            //进行绑定  
            comAirValve.DisplayMember = "Name";//控件显示的列名  
            comAirValve.ValueMember = "ID";//控件值的列名  

            //通过WMI获取COM端口
            DataTable ports = DeviceManager.MulGetHardwareInfo();
            comAirValve.DataSource = ports;
            foreach (DataRow row in ports.Rows)
            {
                if (row[0].ToString().Contains("COM4"))
                {
                    comAirValve.SelectedIndex = ports.Rows.IndexOf(row);
                    connectValvePort();
                }
            }
        }

        private void refreshHeaterPort()
        {
            //进行绑定  
            comHeaterPort.DisplayMember = "Name";//控件显示的列名  
            comHeaterPort.ValueMember = "ID";//控件值的列名  

            //通过WMI获取COM端口
            DataTable ports = DeviceManager.MulGetHardwareInfo();
            comHeaterPort.DataSource = ports;
            foreach (DataRow row in ports.Rows)
            {
                if (row[0].ToString().Contains("COM7"))
                {
                    comHeaterPort.SelectedIndex = ports.Rows.IndexOf(row);
                    connectHeaterPort();
                }
            }
        }

        private void valvePort_Click(object sender, EventArgs e) { refreshValvePort(); }
        private void heaterPort_Click(object sender, EventArgs e) { refreshHeaterPort(); }

        private void connectValvePort()
        {
            spAirValve.PortName = comAirValve.SelectedValue.ToString();
            spAirValve.BaudRate = 9600;
            spAirValve.Parity = Parity.None;
            spAirValve.DataBits = 8;
            spAirValve.StopBits = StopBits.One;
            spAirValve.Handshake = Handshake.None;
            try
            {
                if (!spAirValve.IsOpen) spAirValve.Open();
                comAirValve.BackColor = Color.LightGreen;
                byte[] buffer_init = new byte[4];
                buffer_init[0] = ASCII_ZERO + 8;
                buffer_init[1] = ASCII_ZERO + 10;
                buffer_init[2] = ASCII_ZERO + 0;
                buffer_init[3] = ASCII_ZERO + 2;
                spAirValve.Write(buffer_init, 0, 4);
            }
            catch
            {
                comAirValve.BackColor = Color.FromArgb(255, 192, 192);
            }
        }

        private void connectHeaterPort()
        {
            spHeater.PortName = comHeaterPort.SelectedValue.ToString();
            spHeater.BaudRate = 57600;
            spHeater.Parity = Parity.None;
            spHeater.DataBits = 8;
            spHeater.StopBits = StopBits.One;
            spHeater.Handshake = Handshake.None;
            spHeater.ReadBufferSize = 2048;
            try
            {
                if (!spHeater.IsOpen) spHeater.Open();
                byte[] buffer_init = new byte[2];
                buffer_init[0] = (byte)'w';
                buffer_init[1] = (byte)'\r';
                spHeater.Write(buffer_init, 0, 2);
                spHeater.ReadLine();
                comHeaterPort.BackColor = Color.LightGreen;
                spHeater.Close();
            }
            catch
            {
                comHeaterPort.BackColor = Color.FromArgb(255, 192, 192);
            }
        }

        private void btnConnectValvePort_Click(object sender, EventArgs e)
        {
            connectValvePort();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (spHeater.IsOpen)
                {
                    spHeater.WriteLine("N1#100#1000#1000");
                    spHeater.ReadLine();
                    spHeater.WriteLine("B0");
                    spHeater.ReadLine();
                    spHeater.Close();
                }
                if (spAirValve.IsOpen) spAirValve.Close();
            }
            catch { }

            if (threadPositionMonitor != null && threadPositionMonitor.IsAlive)
            {
                threadPositionMonitor.Abort();
            }
            if (threadTemperatureMonitor != null && threadTemperatureMonitor.IsAlive)
            {
                threadTemperatureMonitor.Abort();
            }
            
            console.EndCtrl();
            // MessageBox.Show("Bye!");
        }

        private void btnModifyFile_Click(object sender, EventArgs e)
        {
            if (cbMode.SelectedValue != null)
            {
                loadData();
                dataGrid.Show();
            }
        }

        private void init_Console()
        {
            if (console.StartCtrl(true, 
            (delegate {
                // Home compelete handler
                readyPrint();
                btnPrint.Enabled = false;
                homeReady = true;
                updatePreference();
                eject(true);
                enableBtnPrint();
                threadPositionMonitor = new Thread(positionMonitor);
                threadPositionMonitor.Start();
                pbTarget.Image = Properties.Resources.target;
                pbTarget.SizeMode = PictureBoxSizeMode.StretchImage;
                pbTarget.BackColor = Color.Transparent;
                pbTarget.Size = new Size(16, 16);
                panelXYStage.Controls.Add(pbTarget);
                psPrint.Visible = false;
            })))
            {
                Invoke((EventHandler)(delegate
                {
                    consoleReady = true;
                    spinner.Visible = false;
                    btnOpenConsole.Enabled = true;
                    btnPrint.Enabled = true;
                    btnEject.Enabled = true;
                    printController = new PrintController(console, spAirValve);
                }));
            }
        }

        private void enableBtnPrint()
        {
            btnPrint.Enabled = ((!homeReady && consoleReady) || (homeReady && fileReady)) && (btnEject.Text == "Eject");
        }

        private void updatePreference()
        {
            Data.xDistance = Convert.ToDouble(tbXDist.Text);
            Data.yDistance = Convert.ToDouble(tbYDist.Text);
            Data.xStart = Convert.ToDouble(tbXStart.Text);
            Data.yStart = Convert.ToDouble(tbYStart.Text);
            Data.idleSpeed = Convert.ToDouble(tbIdleSpeed.Text);
            Data.waitTime = Convert.ToInt32(tbWaitTime.Text);
            Data.xRelative = new double[Data.channelNumber];
            Data.yRelative = new double[Data.channelNumber];
            Data.frequency = new int[Data.channelNumber];
            Data.pulsewidth = new int[Data.channelNumber];
            Data.channel = new int[1];
            Data.channel[0] = Convert.ToInt32(cbChannel.SelectedIndex) + 1;

            for (int i = 0; i < Data.channelNumber; i++)
            {
                Data.xRelative[i] = Convert.ToDouble(tbXRelative.Text);
                Data.yRelative[i] = Convert.ToDouble(tbYRelative.Text);
                Data.frequency[i] = Convert.ToInt32(tbFrequency.Text);
                Data.pulsewidth[i] = Convert.ToInt32(tbPulseWidth.Text);
            }
            Data.workSpeed = (float)(Convert.ToDouble(tbFrequency.Text) * Convert.ToDouble(tbXDist.Text));
            tbWorkSpeed.Text = Data.workSpeed.ToString();
        }

        private void loadFile()
        {
            if (fileLoader.loadCSV())
            {
                updatePreference();
                Invoke((EventHandler)delegate
                {
                    dataGrid = new DataGrid();
                    dataGrid.refreshGrids();
                    btnModifyFile.Enabled = true;
                    fileReady = true;
                    enableBtnPrint();
                });
            }
        }

        private void temperatureMonitor()
        {
            // The reason I use try here is to prevent bug when closing the form. 
            try
            {
                while (true)
                {
                    if (!spHeater.IsOpen)
                    {
                        spHeater.Open();
                    }
                    spHeater.WriteLine("t1");
                    string line = spHeater.ReadLine();                   
                    temperature = Convert.ToDouble(Regex.Matches(line, @"\d*\.\d*")[0].ToString());
                    System.Windows.Forms.DataVisualization.Charting.DataPointCollection points = chartTemperature.Series[0].Points;
                    Invoke((EventHandler)(delegate
                    {
                        if (points.Count > 1000)
                        {
                            points.RemoveAt(0);
                            chartTemperature.Update();
                        }
                        points.AddXY(Math.Round((double)(DateTime.Now - initialTime).TotalMilliseconds / 1000, 1), temperature);

                        double minY = points.FindMinByValue().YValues[0], maxY = points.FindMaxByValue().YValues[0];
                        // +10 is to preserve a minimum range
                        double range = maxY - minY + 10;
                        chartTemperature.ChartAreas[0].AxisY.Minimum = minY - range * 0.2;
                        chartTemperature.ChartAreas[0].AxisY.Maximum = maxY + range * 0.2;

                        lbTemp.Text = temperature.ToString();
                    }));
                    int interval = Convert.ToInt32(trackTempUpdate.Maximum - trackTempUpdate.Value);
                    Thread.Sleep(interval);
                }
            }
            catch { }
        }

        private void tempCommander(string setTime_str, string setTemp_str)
        {
            if (setTime_str == "" || setTime_str == "0")
            {
                return;
            }
            long setTime = Convert.ToInt64(setTime_str);
            int setTemp = Convert.ToInt32(setTemp_str);

            spHeater.WriteLine("H" + setTime.ToString());
            spHeater.ReadLine();
            spHeater.WriteLine("S" + setTemp.ToString());
            spHeater.ReadLine();
            while (Math.Abs(temperature - setTemp) > 1) { }
            DateTime startTime = DateTime.Now;
            while ((DateTime.Now - startTime).TotalMilliseconds / 1000 <= setTime) { }
        }

        private void tempPeratureCycler()
        {
            CYCLE_STAGES stage = CYCLE_STAGES.PRE;
            int setTemp = 0, setCycles = 0;
            long startTime = 0, setTime = 0;

            if (!spHeater.IsOpen)
            {
                spHeater.Open();
            }
            try
            {
                // 初始化设置
                spHeater.WriteLine("N1#255#1000#600");
                spHeater.ReadLine();
                while (stage <= CYCLE_STAGES.EXT)
                {
                    switch (stage)
                    {
                        case CYCLE_STAGES.PRE:
                            tempCommander(stagePRE_time.Text, stagePRE_temp.Text);
                            stagePRE_time.BackColor = Color.PaleGreen;
                            stagePRE_temp.BackColor = Color.PaleGreen;
                            break;
                        case CYCLE_STAGES.EXT:
                            tempCommander(stageEXT_time.Text, stageEXT_temp.Text);
                            stageEXT_time.BackColor = Color.PaleGreen;
                            stageEXT_temp.BackColor = Color.PaleGreen;
                            break;
                        default:
                            if (stage == CYCLE_STAGES.S1)
                            {
                                for (int i = 0; i < Convert.ToInt32(stage1_cycles.Text); i++)
                                {
                                    tempCommander(stage1_1_time.Text, stage1_1_temp.Text);
                                    stage1_1_time.BackColor = Color.PaleGreen;
                                    stage1_1_temp.BackColor = Color.PaleGreen;
                                    tempCommander(stage1_2_time.Text, stage1_2_temp.Text);
                                    stage1_2_time.BackColor = Color.PaleGreen;
                                    stage1_2_temp.BackColor = Color.PaleGreen;
                                    tempCommander(stage1_3_time.Text, stage1_3_temp.Text);
                                    stage1_3_time.BackColor = Color.PaleGreen;
                                    stage1_3_temp.BackColor = Color.PaleGreen;
                                }
                                stage1_cycles.BackColor = Color.PaleGreen;
                                stage1_cycles.BackColor = Color.PaleGreen;
                            }
                            if (stage == CYCLE_STAGES.S2)
                            {
                                for (int i = 0; i < Convert.ToInt32(stage2_cycles.Text); i++)
                                {
                                    tempCommander(stage2_1_time.Text, stage2_1_temp.Text);
                                    stage2_1_time.BackColor = Color.PaleGreen;
                                    stage2_1_temp.BackColor = Color.PaleGreen;
                                    tempCommander(stage2_2_time.Text, stage2_2_temp.Text);
                                    stage2_2_time.BackColor = Color.PaleGreen;
                                    stage2_2_temp.BackColor = Color.PaleGreen;
                                    tempCommander(stage2_3_time.Text, stage2_3_temp.Text);
                                    stage2_3_time.BackColor = Color.PaleGreen;
                                    stage2_3_temp.BackColor = Color.PaleGreen;
                                }
                                stage2_cycles.BackColor = Color.PaleGreen;
                                stage2_cycles.BackColor = Color.PaleGreen;
                            }
                            break;
                    }
                    stage++;
                }

            }
            catch { }
            finally
            {
                if (!spHeater.IsOpen)
                {
                    spHeater.Open();
                }
                spHeater.WriteLine("N1#100#1000#1000");
                spHeater.ReadLine();
                spHeater.WriteLine("B0");
                spHeater.ReadLine();
                stagePRE_time.BackColor = Color.White; stagePRE_temp.BackColor = Color.White;
                stageEXT_time.BackColor = Color.White; stageEXT_temp.BackColor = Color.White;
                stage1_1_time.BackColor = Color.White; stage1_1_temp.BackColor = Color.White;
                stage1_2_time.BackColor = Color.White; stage1_2_temp.BackColor = Color.White;
                stage1_3_time.BackColor = Color.White; stage1_3_temp.BackColor = Color.White;
                stage2_1_time.BackColor = Color.White; stage2_1_temp.BackColor = Color.White;
                stage2_2_time.BackColor = Color.White; stage2_2_temp.BackColor = Color.White;
                stage2_3_time.BackColor = Color.White; stage2_3_temp.BackColor = Color.White;
                stage1_cycles.BackColor = Color.White; stage1_cycles.BackColor = Color.White;
                stage2_cycles.BackColor = Color.White; stage2_cycles.BackColor = Color.White;
                btnRunCycle.Text = "Run Cycles";
            }
        }

        private void positionMonitor()
        {
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            float x = 0, y = 0;

            while (true)
            {
                Thread.Sleep(10);
                try
                {
                    console.getXPosition(ref x);
                    console.getYPosition(ref y);
                    Invoke((EventHandler)(delegate
                    {
                        graphicsXYStage.Clear(Color.White);

                        pbTarget.Location = new Point(
                            (int)(x / console.getXAxisMax() * panelXYStage.Size.Width - pbTarget.Size.Width / 2.0),
                            (int)((1 - y / console.getYAxisMax()) * panelXYStage.Size.Height - pbTarget.Size.Height / 2.0));
                        if (printState == PRINT_STATE.STOPPED)
                        {
                            if (ejectState == EJECT_STATE.INPOSITION)
                            {
                                float dataWidth = (Data.cols - 1) * (float)Data.xDistance / console.getXAxisMax() * panelXYStage.Size.Width;
                                float dataHeight = (Data.rows - 1) * (float)Data.yDistance / console.getYAxisMax() * panelXYStage.Size.Height;
                                float bufferWidth = (float)(Data.workSpeed * Data.workSpeed / Data.workAccn / 2);
                                float bufferWidthDisp = bufferWidth / console.getXAxisMax() * panelXYStage.Size.Width;
                                Color printColor = new Color(), bufferColor = new Color();
                                if ((console.getXAxisMax() - x <= (Data.xDistance * (Data.cols - 1) + bufferWidth))
                                    || (x - bufferWidth < 0)
                                    || (y <= (Data.yDistance * (Data.rows - 1))))
                                {
                                    printColor = Color.FromArgb(255, 153, 153);
                                    bufferColor = Color.FromArgb(255, 153, 153);
                                }
                                else
                                {
                                    printColor = Color.FromArgb(153, 255, 153);
                                    bufferColor = Color.FromArgb(255, 255, 153);
                                }

                                string hint = "(" + Math.Round(x, 2).ToString() + "," + Math.Round(y, 2).ToString() + ")";
                                if (!fileReady)
                                {
                                    hint += " No Data";
                                }
                                else
                                {
                                    float fontSize = dataWidth / 300 * 14;
                                    fontSize = fontSize < 5 ? 5 : fontSize;
                                    graphicsXYStage.FillRectangle(new SolidBrush(printColor),
                                    pbTarget.Location.X + pbTarget.Size.Width / 2,
                                    pbTarget.Location.Y + pbTarget.Size.Height / 2, dataWidth, dataHeight);
                                    graphicsXYStage.FillRectangle(new SolidBrush(bufferColor),
                                    pbTarget.Location.X + pbTarget.Size.Width / 2 - bufferWidthDisp,
                                    pbTarget.Location.Y + pbTarget.Size.Height / 2, bufferWidthDisp, dataHeight);
                                    graphicsXYStage.FillRectangle(new SolidBrush(bufferColor),
                                    pbTarget.Location.X + pbTarget.Size.Width / 2 + dataWidth,
                                    pbTarget.Location.Y + pbTarget.Size.Height / 2, bufferWidthDisp, dataHeight);
                                    graphicsXYStage.DrawString("PRINT AREA",
                                    new Font("Arial", fontSize),
                                    new SolidBrush(Color.Black),
                                    new PointF(pbTarget.Location.X + dataWidth / 2, pbTarget.Location.Y + dataHeight / 2),
                                    format);
                                }
                                graphicsXYStage.DrawString(hint,
                                    new Font("Arial", 10),
                                    new SolidBrush(Color.Black),
                                    new PointF(pbTarget.Location.X, pbTarget.Location.Y - 15),
                                    format);
                            }
                            else if (ejectState == EJECT_STATE.EJECTED)
                            {
                                graphicsXYStage.DrawString("Please insert the chip.",
                                    new Font("Arial", 16),
                                    new SolidBrush(Color.Black),
                                    panelXYStage.Size.Width / 2, panelXYStage.Size.Height / 2,
                                    format);
                            }
                        }
                    }));
                }
                catch {
                    return;
                }
            }
        }

        private void btnEject_Click(object sender, EventArgs e)
        {
            updatePreference();
            if (btnEject.Text == "Eject")
            {
                eject(true);
            }
            else
            {
                eject(false);
            }
        }

        private void rememberPosition()
        {
            if (cbFromCurrent.Checked)
            {
                console.MotorX.GetPosition(0, ref savedX);
                console.MotorY.GetPosition(0, ref savedY);
                tbXStart.Text = savedX.ToString();
                tbYStart.Text = savedY.ToString();
                updatePreference();
            }
        }
        private void eject(bool ejectDirection)
        {
            if (ejectDirection)
            {
                // Eject
                if (ejectState == EJECT_STATE.EJECTED) return;
                rememberPosition();
                printController.eject();
                btnEject.Text = "In Position";
                ejectState = EJECT_STATE.EJECTED;
                pbTarget.Visible = false;
                enableBtnPrint();
            }
            else
            {
                // Go to position
                if (ejectState == EJECT_STATE.INPOSITION) return;
                switch (printState)
                {
                    case PRINT_STATE.STOPPED: printController.inPosition(); break;
                    case PRINT_STATE.SUSPENDED: printController.inPosition(); break;
                }
                btnEject.Text = "Eject";
                ejectState = EJECT_STATE.INPOSITION;
                pbTarget.Visible = true;
                enableBtnPrint();
            }
        }

        private void beginPrint()
        {
            int mode = 0;
            Invoke((EventHandler)(delegate
            {
                mode = cbPrintMode.SelectedIndex;
                int padding = 10;
                float sideLength = (float)((1.0 * (panelXYStage.Height - padding * 2) / Data.rows)
                                     < (1.0 * (panelXYStage.Width - padding * 2) / Data.cols)
                                     ? (1.0 * (panelXYStage.Height - padding * 2) / Data.rows)
                                     : (1.0 * (panelXYStage.Width - padding * 2) / Data.cols));
                sideLength /= 2;
                float leftSide = panelXYStage.Width / 2 - sideLength * Data.cols + padding;
                float upSide = panelXYStage.Height / 2 - sideLength * Data.rows + padding;

                panelXYStage.Controls.Clear();
                graphicsXYStage.Clear(Color.White);
                if (Data.rows * Data.cols <= 500)
                {
                    for (int i = 0; i < Data.rows; i++)
                    {
                        for (int j = 0; j < Data.cols; j++)
                        {
                            if ((int)((ArrayList)((ArrayList)Data.gridData[0])[i])[j] != 0)
                            {
                                PictureBox pb = (PictureBox)((ArrayList)((ArrayList)Data.dataImages[0])[i])[j];
                                pb.Image = Properties.Resources.circle_black;
                                pb.SizeMode = PictureBoxSizeMode.StretchImage;
                                pb.BackColor = Color.Transparent;
                                pb.Size = new Size((int)(sideLength), (int)(sideLength));
                                pb.Location = new Point((int)(j * 2 * sideLength + leftSide), (int)(i * 2 * sideLength + upSide));
                                panelXYStage.Controls.Add(pb);
                            }
                        }
                    }
                }
                else
                {
                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    graphicsXYStage.DrawString("Unable to show. Too many points.",
                                new Font("Arial", 16),
                                new SolidBrush(Color.Black),
                                panelXYStage.Size.Width / 2, panelXYStage.Size.Height / 2,
                                format);
                }

                if (threadPositionMonitor != null && threadPositionMonitor.IsAlive)
                    threadPositionMonitor.Suspend();
            }));
            Thread.Sleep(100);
            stopwatch.Reset();
            stopwatch.Start();
            printController.beginPrint(
                mode, 
                (delegate {
                    Invoke((EventHandler)(delegate
                    {
                        stopPrint();
                    }));
                }),
                (delegate (object sender, EventArgs e) {
                    int row = (int)((ArrayList)sender)[0];
                    int col = (int)((ArrayList)sender)[1];
                    Invoke((EventHandler)(delegate
                    {
                        graphicsXYStage.Clear(Color.White);
                        int progress = (int)(100.0 * (row * Data.cols + col + 1) / (Data.rows * Data.cols));
                        lbPrintProgress.Text = progress.ToString() + '%';
                        pbPrint.Value = progress;
                        lbTimeLeft.Text = Math.Round((1.0 * timeUsed / progress * (100 - progress)) / 1000, 2).ToString() + 's';
                        PictureBox pb = (PictureBox)((ArrayList)((ArrayList)Data.dataImages[0])[row])[col];
                        pb.Image = Properties.Resources.circle_filled;
                    }));
                })
            );
        }

        private void stopPrint()
        {
            printState = PRINT_STATE.STOPPED;
            stopwatch.Stop();
            if (threadPositionMonitor != null && threadPositionMonitor.IsAlive)
                threadPositionMonitor.Resume();
            btnEject.Enabled = true;
            readyPrint();
            eject(true);
            lbPrintProgress.Text = "0%";
            pbPrint.Value = 0;
            panelXYStage.Controls.Clear();
        }

        private void readyPrint()
        {
            btnPrint.TileImage = Properties.Resources.print;
            btnPrint.Text = "Print";
            cbFromCurrent.Visible = true;
        }

        private void cbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMode.SelectedValue != null)
            {
                loadData();
            }
        }

        private void cbMode_Click(object sender, EventArgs e)
        {
            refreshMode();
        }

        private void loadData()
        {
            Data.clear();
            Data.ExcelSafeFileName = cbMode.Text;
            Data.ExcelFileName = (string)cbMode.SelectedValue;
            loadFile();
            //threadFile = new Thread(loadFile);
            //threadFile.SetApartmentState(ApartmentState.STA);
            //threadFile.Start();
        }

        private void refreshMode()
        {
            int index = cbMode.SelectedIndex;
            //进行绑定  
            cbMode.DisplayMember = "SafeName";//控件显示的列名  
            cbMode.ValueMember = "FullName";//控件值的列名  

            DirectoryInfo folder = new DirectoryInfo(Application.StartupPath + @"\WorkData\");

            DataTable ADt = new DataTable();
            DataColumn ADC1 = new DataColumn("SafeName", typeof(string));
            DataColumn ADC2 = new DataColumn("FullName", typeof(string));
            ADt.Columns.Add(ADC1);
            ADt.Columns.Add(ADC2);
            foreach (FileInfo file in folder.GetFiles("*.csv"))
            {
                DataRow ADR = ADt.NewRow();
                ADR[0] = file.Name;
                ADR[1] = file.FullName;
                ADt.Rows.Add(ADR);
            }
            cbMode.DataSource = ADt;
            cbMode.SelectedIndex = index;
        }

        private void refreshFrequency()
        {
            graphicsFrequency.Clear(Color.White);
            float frequency = (float)Convert.ToDouble(tbFrequency.Text);
            float pulseWidth = (float)Convert.ToDouble(tbPulseWidth.Text);
            float padding = 5;
            float fullWidth = panelFrequency.Width - padding * 2;
            int displayPulseCount = 3;
            float perPulseWidth = fullWidth / displayPulseCount;
            float lowY = panelFrequency.Height - 5, highY = panelFrequency.Height / 3;
            for (int i = 0; i < displayPulseCount; i++)
            {
                float firstX = padding + perPulseWidth * i;
                float midX = (float)(padding + perPulseWidth * (i + 0.01 * pulseWidth));
                float lastX = padding + perPulseWidth * (i + 1);
                graphicsFrequency.DrawLine(new Pen(Color.Black, 1),
                    new PointF(firstX, highY), new PointF(firstX, lowY));
                graphicsFrequency.DrawLine(new Pen(Color.Black, 1),
                    new PointF(firstX, highY), new PointF(midX, highY));
                graphicsFrequency.DrawLine(new Pen(Color.Black, 1),
                    new PointF(midX, highY), new PointF(midX, lowY));
                graphicsFrequency.DrawLine(new Pen(Color.Black, 1),
                    new PointF(midX, lowY), new PointF(lastX, lowY));
                graphicsFrequency.DrawString((Math.Round((1000 / frequency * 0.01 * pulseWidth), 2)).ToString() + "ms",
                                new Font("Arial", 6),
                                new SolidBrush(Color.Black),
                                new PointF(firstX, 0));
            }
            updatePreference();
        }

        private void tbPulseWidth_TextChanged(object sender, EventArgs e)
        {
            if (tbPulseWidth.Text == "") tbPulseWidth.Text = "0";
            if (Convert.ToInt32(tbPulseWidth.Text) > 100) tbPulseWidth.Text = "100";
            refreshFrequency();
        }

        private void btnPreviewPulse_Click(object sender, EventArgs e)
        {
            refreshFrequency();
        }

        private void togXAxis_CheckedChanged(object sender, EventArgs e)
        {
            if (homeReady)
            {
                console.setXEnabled(togXAxis.Checked);
                PrinterPro.Properties.Settings.Default.X_Enable = togXAxis.Checked;
                PrinterPro.Properties.Settings.Default.Save();
                if (togXAxis.Checked)
                {
                    console.moveXHome();
                }
            }
        }

        private void togYAxis_CheckedChanged(object sender, EventArgs e)
        {
            if (homeReady)
            {
                console.setYEnabled(togYAxis.Checked);
                PrinterPro.Properties.Settings.Default.Y_Enable = togYAxis.Checked;
                PrinterPro.Properties.Settings.Default.Save();
                if (togYAxis.Checked)
                {
                    console.moveYHome();
                }
            }
        }

        private void togZAxis_CheckedChanged(object sender, EventArgs e)
        {
            if (homeReady)
            {
                console.setZEnabled(togZAxis.Checked);
                PrinterPro.Properties.Settings.Default.Z_Enable = togZAxis.Checked;
                PrinterPro.Properties.Settings.Default.Save();
                if (togZAxis.Checked)
                {
                    console.moveZHome();
                }
            }
        }

        private void panelFrequency_Paint(object sender, PaintEventArgs e)
        {
            refreshFrequency();
        }

        private void btnDeleteMode_Click(object sender, EventArgs e)
        {
            if (Data.ExcelFileName != "")
            {
                FileAttributes attr = File.GetAttributes(Data.ExcelFileName);
                if (attr == FileAttributes.Directory)
                {
                    Directory.Delete(Data.ExcelFileName, true);
                }
                else
                {
                    File.Delete(Data.ExcelFileName);
                }
                cbMode.SelectedIndex = 0;
                Data.clear();
                Data.ExcelSafeFileName = cbMode.Text;
                Data.ExcelFileName = (string)cbMode.SelectedValue;
                threadFile = new Thread(loadFile);
                threadFile.SetApartmentState(ApartmentState.STA);
                threadFile.Start();
            }
        }

        private void cbChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            updatePreference();
        }

        private void btnConnectHeaterPort_Click(object sender, EventArgs e)
        {
            connectHeaterPort();
        }

        private void btnRunCycle_Click(object sender, EventArgs e)
        {
            if (threadTemperatureController != null && threadTemperatureController.IsAlive)
            {
                btnRunCycle.Text = "Run Cycles";
                threadTemperatureController.Abort();
            }
            else
            {
                try
                {
                    if (!spHeater.IsOpen)
                    {
                        spHeater.Open();
                    }
                    threadTemperatureController = new Thread(tempPeratureCycler);
                    threadTemperatureController.Start();
                    btnRunCycle.Text = "Stop Cycles";
                }
                catch
                { }
            }
        }

        private void btnTempMonitor_Click(object sender, EventArgs e)
        {
            if (threadTemperatureMonitor != null && threadTemperatureMonitor.IsAlive)
            {
                threadTemperatureMonitor.Abort();
                btnTempMonitor.Text = "Monitor Begin";
            }
            else
            {
                try
                {
                    if (!spHeater.IsOpen)
                    {
                        spHeater.Open();
                    }
                    threadTemperatureMonitor = new Thread(temperatureMonitor);
                    threadTemperatureMonitor.Start();
                    btnTempMonitor.Text = "Stop";
                }
                catch
                {}
            }
        }

        private void tbFrequency_TextChanged(object sender, EventArgs e)
        {
            if (tbFrequency.Text == "") tbFrequency.Text = "0";
            refreshFrequency();
        }

        private void btnAddMode_Click(object sender, EventArgs e)
        {
            AddMode am = new AddMode();
            am.Show();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            updatePreference();
            printController.stop();
            if (threadPrint != null)
            {
                if (threadPrint.ThreadState == System.Threading.ThreadState.Suspended)
                {
                    threadPrint.Resume();
                }
                threadPrint.Abort();
            }
            stopPrint();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (!homeReady)
            {
                console.MoveHome((delegate
                {
                    readyPrint();
                    btnPrint.Enabled = false;
                    homeReady = true;
                    printController.eject();
                    enableBtnPrint();
                }));
            }
            else
            {
                float workSpeed = (float)(Convert.ToDouble(tbFrequency.Text) * Convert.ToDouble(tbXDist.Text));
                tbWorkSpeed.Text = workSpeed.ToString();
                updatePreference();
                switch (printState)
                {
                    case PRINT_STATE.STOPPED:
                        #region load Data File First
                        loadData();
                        #endregion
                        rememberPosition();
                        eject(false);
                        printState = PRINT_STATE.PRINTING;
                        threadPrint = new Thread(beginPrint);
                        threadPrint.Start();
                        btnPrint.TileImage = Properties.Resources.pause;
                        btnPrint.Text = "Pause";
                        btnEject.Enabled = false;
                        break;
                    case PRINT_STATE.SUSPENDED:
                        eject(false);
                        stopwatch.Reset();
                        stopwatch.Start();
                        threadPrint.Resume();
                        printState = PRINT_STATE.PRINTING;
                        btnPrint.TileImage = Properties.Resources.pause;
                        btnPrint.Text = "Pause";
                        btnEject.Enabled = false;
                        break;
                    case PRINT_STATE.PRINTING:
                        threadPrint.Suspend();
                        stopwatch.Stop();
                        printState = PRINT_STATE.SUSPENDED;
                        btnPrint.TileImage = Properties.Resources.play;
                        btnPrint.Text = "Resume";
                        btnEject.Enabled = true;
                        break;
                }
            }
        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            refreshValvePort();
            refreshHeaterPort();
            refreshFrequency();

            console = new Console(
                PrinterPro.Properties.Settings.Default.X_Enable,
                PrinterPro.Properties.Settings.Default.Y_Enable,
                PrinterPro.Properties.Settings.Default.Z_Enable);
            threadConsole = new Thread(init_Console);
            threadConsole.Start();
        }

        // Open File Button Clicked
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string setting_path = Application.StartupPath + @"\WorkData\";
            ofd.InitialDirectory = setting_path;
            ofd.Title = "Load an excel file";
            ofd.FileName = "";
            ofd.Filter = "CSV文件(*.csv)|*.csv";
            if (ofd.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            string sourceFile = ofd.FileName;
            string destinationFile = setting_path + ofd.SafeFileName;
            bool isrewrite = true; // true=覆盖已存在的同名文件,false则反之
            File.Copy(sourceFile, destinationFile, isrewrite);

            Data.clear();
            Data.ExcelFileName = destinationFile;
            Data.ExcelSafeFileName = ofd.SafeFileName;
            
            threadFile = new Thread(loadFile);
            threadFile.SetApartmentState(ApartmentState.STA);
            threadFile.Start();
        }

        private void btnOpenConsole_Click(object sender, EventArgs e)
        {
            console.StartPosition = FormStartPosition.CenterParent;
            console.Show();
        }
    }
}
