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
            this.Text = Data.ExcelSafeFileName;
            for (int i = 0; i < Data.SolutionNumber; i++)
            {
                TabPage page = new TabPage((string)Data.sheetNames[i]);
                DataTable dataTable = new DataTable();
                MetroGrid grid = new MetroGrid();

                grid.Dock = DockStyle.Fill;

                for (int j = 0; j < Data.colCount; j++)
                {
                    dataTable.Columns.Add();
                }
                for (int k = 0; k < Data.rowCount; k++)
                {
                    DataRow row = dataTable.NewRow();
                    for (int j = 0; j < Data.colCount; j++)
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
    }
}
