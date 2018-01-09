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
using System.Configuration;

namespace PrinterPro
{
    public partial class MainForm : MetroForm
    {
        #region variables

        private enum PRINT_STATE { PRINTING, SUSPENDED, STOPPED };
        private enum EJECT_STATE { INITIAL, EJECTED, INPOSITION };
        private PRINT_STATE printState = PRINT_STATE.STOPPED;
        private EJECT_STATE ejectState = EJECT_STATE.INITIAL;

        private const int ASCII_ZERO = 48;
        public ManualResetEvent pauseEvent = new ManualResetEvent(true);
        public ManualResetEvent stopEvent = new ManualResetEvent(true);
        public SerialPort _serialPort = new SerialPort();
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

        public Thread threadPrint = null, threadConsole = null, threadFile = null, threadPositionMonitor = null;
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

            console = new Console(
                Properties.Settings.Default.X_Enable,
                Properties.Settings.Default.Y_Enable,
                Properties.Settings.Default.Z_Enable);

            togXAxis.Checked = Properties.Settings.Default.X_Enable;
            togYAxis.Checked = Properties.Settings.Default.Y_Enable;
            togZAxis.Checked = Properties.Settings.Default.Z_Enable;
            cbPrintMode.SelectedIndex = Properties.Settings.Default.PrintMode;
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
                if (row[0].ToString().Contains("USB Serial Port"))
                {
                    comAirValve.SelectedIndex = ports.Rows.IndexOf(row);
                    connectValvePort();
                }
            }
        }

        private void refreshHeaterPort()
        {
            //进行绑定  
            this.comHeaterPort.DisplayMember = "Name";//控件显示的列名  
            this.comHeaterPort.ValueMember = "ID";//控件值的列名  

            //通过WMI获取COM端口
            this.comHeaterPort.DataSource = DeviceManager.MulGetHardwareInfo();
        }

        private void valvePort_Click(object sender, EventArgs e) { refreshValvePort(); }
        private void heaterPort_Click(object sender, EventArgs e) { refreshHeaterPort(); }

        private void connectValvePort()
        {
            _serialPort.PortName = comAirValve.SelectedValue.ToString();
            _serialPort.BaudRate = 9600;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.None;
            try
            {
                if (_serialPort.IsOpen) _serialPort.Close();
                _serialPort.Open();
                comAirValve.BackColor = Color.LightGreen;
                byte[] buffer_init = new byte[4];
                buffer_init[0] = ASCII_ZERO + 8;
                buffer_init[1] = ASCII_ZERO + 10;
                buffer_init[2] = ASCII_ZERO + 0;
                buffer_init[3] = ASCII_ZERO + 2;
                _serialPort.Write(buffer_init, 0, 4);
                _serialPort.Close();
            }
            catch
            {
                comAirValve.BackColor = Color.FromArgb(255, 192, 192);
            }
        }
        private void btnConnectValvePort_Click(object sender, EventArgs e)
        {
            connectValvePort();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (threadPositionMonitor != null && threadPositionMonitor.IsAlive)
            {
                threadPositionMonitor.Abort();
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
            if (console.StartCtrl(true, (delegate
            {
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
                    printController = new PrintController(console, _serialPort);
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

        private void positionMonitor()
        {
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            float x = 0, y = 0;

            while (true)
            {
                Thread.Sleep(10);
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
                if (Data.rows * Data.cols <= 10000)
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

        private void togXAxis_Click(object sender, EventArgs e)
        {
            if (togXAxis.Checked)
            {
                console.moveXHome();
            }
        }

        private void togYAxis_Click(object sender, EventArgs e)
        {
            if (togYAxis.Checked)
            {
                console.moveYHome();
            }
        }

        private void togZAxis_Click(object sender, EventArgs e)
        {
            if (togZAxis.Checked)
            {
                console.moveZHome();
            }
        }

        private void togXAxis_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.X_Enable = togXAxis.Checked;
            Properties.Settings.Default.Save();
        }

        private void togYAxis_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Y_Enable = togYAxis.Checked;
            Properties.Settings.Default.Save();
        }

        private void togZAxis_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Z_Enable = togZAxis.Checked;
            Properties.Settings.Default.Save();
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
            threadConsole = new Thread(init_Console);
            threadConsole.Start();

            refreshValvePort();
            refreshHeaterPort();
            refreshFrequency();
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
