using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Threading;
using System.IO.Ports;

namespace PrinterPro
{
    public partial class MainForm : MetroForm
    {
        #region variables

        private enum STAGE { PRINTING, SUSPENDED, STOPPED};
        private STAGE stage = STAGE.STOPPED;

        private const int ASCII_ZERO = 48;
        public static ManualResetEvent pauseEvent = new ManualResetEvent(true);
        public static ManualResetEvent stopEvent = new ManualResetEvent(true);
        public static SerialPort _serialPort = new SerialPort();
        private static Console console = new Console();
        private static DataGrid dataGrid = new DataGrid();
        private static FileLoader fileLoader = new FileLoader();
        private static PrintController printController;

        float savedX = 0, savedY = 0;

        private bool homeReady = false, fileReady = false, consoleReady = false;

        public Thread threadPrint = null, threadConsole = null, threadFile = null;
        #endregion


        public MainForm()
        {
            InitializeComponent();
            refreshValvePort();
            refreshHeaterPort();
            tabControl.SelectedIndex = 0;

            cbFromCurrent.Visible = false;
            btnOpenConsole.Enabled = false;
            btnPrint.Enabled = false;
            btnEject.Enabled = false;
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
            console.EndCtrl();
            //MessageBox.Show("Bye!");
        }

        private void btnModifyFile_Click(object sender, EventArgs e)
        {
            dataGrid.Show();
        }

        private void init_Console()
        {
            if (console.StartCtrl(true, (delegate
            {
                readyPrint();
                btnPrint.Enabled = false;
                homeReady = true;
                enableBtnPrint();
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
            btnPrint.Enabled = ((!homeReady && consoleReady) || (homeReady && fileReady));
        }

        private void updatePreference()
        {
            Data.xDistance = Convert.ToDouble(tbXDist.Text);
            Data.yDistance = Convert.ToDouble(tbYDist.Text);
            Data.xStart = Convert.ToDouble(tbXStart.Text);
            Data.yStart = Convert.ToDouble(tbYStart.Text);
            Data.idleSpeed = Convert.ToDouble(tbIdleSpeed.Text);
            Data.workSpeed = Convert.ToDouble(tbWorkSpeed.Text);
            Data.waitTime = Convert.ToInt32(tbWaitTime.Text);
            for (int i = 0; i < Data.SolutionNumber; i++)
            {
                Data.xRelative[i] = Convert.ToDouble(tbXRelative.Text);
                Data.yRelative[i] = Convert.ToDouble(tbYRelative.Text);
                Data.frequency[i] = Convert.ToInt32(tbFrequency.Text);
                Data.pulsewidth[i] = Convert.ToInt32(tbPulseWidth.Text);
            }
        }

        private void loadFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string setting_path = Application.StartupPath + @"\WorkData\";
            ofd.InitialDirectory = setting_path;
            ofd.Title = "Load an excel file";
            ofd.FileName = "";
            ofd.Filter = "Excel Files(2007/2010/2013)|*.xlsx|Excel Files(2003)|*.xls";
            if (ofd.ShowDialog() == DialogResult.Cancel)
                return;

            Invoke((EventHandler)(delegate {
                lbFileName.Text = "Please wait...";
                fileReady = false;
                enableBtnPrint();
            }));
            fileLoader.load(ofd.FileName, ofd.SafeFileName);
            Invoke((EventHandler)(delegate
            {
                dataGrid = new DataGrid();
                cbSolution.Items.Clear();
                foreach (string name in Data.sheetNames)
                {
                    cbSolution.Items.Add(name);
                }
                lbFileName.Text = Data.ExcelSafeFileName;
                dataGrid.refreshGrids();
                btnModifyFile.Enabled = true;

                fileReady = true;
                enableBtnPrint();
            }));
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
                if (btnEject.Text == "In Position") return;
                btnPrint.Enabled = false;
                rememberPosition();
                printController.eject();
                btnEject.Text = "In Position";
            }
            else
            {
                // Go to position
                if (btnEject.Text == "Eject") return;
                switch (stage)
                {
                    case STAGE.STOPPED: printController.inPosition(true); break;
                    case STAGE.SUSPENDED: printController.inPosition(false); break;
                }
                btnPrint.Enabled = true;
                btnEject.Text = "Eject";
            }
        }

        private void beginPrint()
        {
            printController.beginPrint((delegate
            {
                stage = STAGE.STOPPED;
                Invoke((EventHandler)(delegate
                {
                    btnEject.Enabled = true;
                    readyPrint();
                    eject(true);
                }));
            }));
        }

        private void readyPrint()
        {
            btnPrint.TileImage = Properties.Resources.print;
            btnPrint.Text = "Print";
            cbFromCurrent.Visible = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            updatePreference();
            printController.stop();
            if (threadPrint != null)
            {
                if (threadPrint.ThreadState == ThreadState.Suspended)
                {
                    threadPrint.Resume();
                }
                stage = STAGE.STOPPED;
                btnEject.Enabled = true;
                eject(true);
                threadPrint.Abort();
                readyPrint();
            }
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
                    enableBtnPrint();
                }));
            }
            else
            {
                updatePreference();
                switch (stage)
                {
                    case STAGE.STOPPED:
                        rememberPosition();
                        eject(false);
                        stage = STAGE.PRINTING;
                        threadPrint = new Thread(beginPrint);
                        threadPrint.Start();
                        btnPrint.TileImage = Properties.Resources.pause;
                        btnPrint.Text = "Pause";
                        btnEject.Enabled = false;
                        break;
                    case STAGE.SUSPENDED:
                        eject(false);
                        threadPrint.Resume();
                        stage = STAGE.PRINTING;
                        btnPrint.TileImage = Properties.Resources.pause;
                        btnPrint.Text = "Pause";
                        btnEject.Enabled = false;
                        break;
                    case STAGE.PRINTING:
                        threadPrint.Suspend();
                        stage = STAGE.SUSPENDED;
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
        }

        // Open File Button Clicked
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
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
