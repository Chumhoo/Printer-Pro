using MetroFramework.Forms;
using System;

namespace PrinterPro
{
    /// <summary>
    /// 快捷添加新csv文件窗体类
    /// </summary>
    public partial class AddMode : MetroForm
    {
        public AddMode()
        {
            InitializeComponent();
        }

        #region 确认按键
        private void btnConfirm_Click(object sender, System.EventArgs e)
        {
            // 创建并保存新的CSV文件
            if (tbFileName.Text != "" && tbRows.Text != "" && tbColumns.Text != "" && tbDroplets.Text != "" )
            {
                FileLoader loader = new FileLoader();
                loader.saveCSV(tbFileName.Text + ".csv", Convert.ToInt32(tbRows.Text),
                    Convert.ToInt32(tbColumns.Text), Convert.ToInt32(tbDroplets.Text));
                // 操作完成后析构窗口
                Dispose();
            }
        }
        #endregion

        #region 取消按键
        private void btnCancel_Click(object sender, EventArgs e)
        {
            // 操作完成后析构窗口
            Dispose();
        }
        #endregion
    }
}
