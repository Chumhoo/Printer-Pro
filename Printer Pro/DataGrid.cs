using System.Data;
using System.Windows.Forms;
using MetroFramework.Forms;
using System.Collections;
using MetroFramework.Controls;

namespace PrinterPro
{
    public partial class DataGrid : MetroForm
    {
        public DataGrid()
        {
            InitializeComponent();
        }

        public void refreshGrids()
        {
            Text = Data.ExcelSafeFileName;
            for (int i = 0; i < Data.channelNumber; i++)
            {
                TabPage page = new TabPage((string)Data.sheetNames[i]);
                DataTable dataTable = new DataTable();
                MetroGrid grid = new MetroGrid();

                grid.Dock = DockStyle.Fill;

                for (int j = 0; j < Data.cols; j++)
                {
                    dataTable.Columns.Add();
                }
                for (int k = 0; k < Data.rows; k++)
                {
                    DataRow row = dataTable.NewRow();
                    for (int j = 0; j < Data.cols; j++)
                    {
                        row[j] = ((ArrayList)((ArrayList)Data.gridData[i])[k])[j];                        
                    }
                    dataTable.Rows.Add(row);
                }

                grid.DataSource = dataTable;
                page.Controls.Add(grid);
                tcData.TabPages.Add(page);
            }
        }

        public void saveGrid()
        {
            System.IO.FileStream fs = new System.IO.FileStream(Data.ExcelFileName,
                System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            System.IO.StreamWriter sr = new System.IO.StreamWriter(fs);

            for (int i = 0; i < tcData.TabPages.Count; i++)
            {
                TabPage page = tcData.TabPages[i];
                MetroGrid grid = (MetroGrid)page.Controls[0];
                DataTable dataTable = (DataTable)grid.DataSource;

                grid.Dock = DockStyle.Fill;

                foreach (DataRow row in dataTable.Rows)
                {
                    if (row.RowState == DataRowState.Deleted) continue;
                    string line = "";
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        line += row[j].ToString();
                        if (j < dataTable.Columns.Count - 1)
                        {
                            line += ",";
                        }
                        else
                        {
                            line += "\n";
                        }
                    }
                    sr.Write(line);
                }
            }
            sr.Close();
            fs.Close();
        }

        private void btnSave_Click(object sender, System.EventArgs e)
        {
            saveGrid();
            Dispose();
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            Dispose();
        }
    }
}
