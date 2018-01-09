using MetroFramework.Forms;
using System;

namespace PrinterPro
{
    public partial class AddMode : MetroForm
    {
        public AddMode()
        {
            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, System.EventArgs e)
        {
            if (tbFileName.Text != "" && tbRows.Text != "" && tbColumns.Text != "" && tbDroplets.Text != "" )
            {
                FileLoader loader = new FileLoader();
                loader.saveCSV(tbFileName.Text + ".csv", Convert.ToInt32(tbRows.Text),
                    Convert.ToInt32(tbColumns.Text), Convert.ToInt32(tbDroplets.Text));
                Dispose();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Dispose();
        }
    }
}
