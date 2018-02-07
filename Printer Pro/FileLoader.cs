using System;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms;
using System.Collections;

namespace PrinterPro
{
    /// <summary>
    /// 文件读取类
    /// </summary>
    class FileLoader
    {
        #region CSV文件操作
        /// <summary>
        /// 读取CSV文件
        /// </summary>
        /// <returns></returns>
        public bool loadCSV()
        {   
            Data.channelNumber = 1;
            DataGrid[] dtArray = new DataGrid[Data.channelNumber];
            ArrayList channelLoc = new ArrayList();

            // 文件输入输出流
            System.IO.FileStream fs = new System.IO.FileStream(Data.ExcelFileName, 
                System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.StreamReader sr = new System.IO.StreamReader(fs);
            string line = "";
            bool IsFirst = true;

            ArrayList channelData = new ArrayList();
            ArrayList channelImages = new ArrayList();
            channelLoc.Add(0);
            Data.sheetNames.Add("Channel 1");
            Data.rows = 0;

            while ((line = sr.ReadLine()) != null)
            {
                if (line == "")
                {
                    break;
                }
                Data.rows += 1;
                ArrayList row = new ArrayList();
                ArrayList rowImages = new ArrayList();
                if (IsFirst == true)
                {
                    // 使用‘,’对数据行进行分割处理
                    Data.cols = line.Split(',').Length;
                    IsFirst = false;
                }
                
                for (int i = 0; i < Data.cols; i++)
                {
                    string[] data;
                    data = line.Split(',');
                    row.Add(Convert.ToInt32(data[i]));
                    rowImages.Add(new PictureBox());
                }
                channelData.Add(row);
                channelImages.Add(rowImages);
            }
            Data.gridData.Add(channelData);
            Data.dataImages.Add(channelImages);
            Data.channel = (int[])channelLoc.ToArray(typeof(int));

            sr.Close();
            fs.Close();

            return true;
        }

        /// <summary>
        /// 保存CSV文件
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="rows">行数</param>
        /// <param name="cols">列数</param>
        /// <param name="countPerDrop">每个点的液滴数</param>
        /// <returns></returns>
        public bool saveCSV(string fileName, int rows, int cols, int countPerDrop)
        {
            string setting_path = Application.StartupPath + @"\WorkData\";
            System.IO.FileStream fs = new System.IO.FileStream(setting_path + fileName,
                System.IO.FileMode.CreateNew, System.IO.FileAccess.Write);
            System.IO.StreamWriter sr = new System.IO.StreamWriter(fs);
            
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    sr.Write(countPerDrop.ToString());
                    if (j < cols - 1) sr.Write(',');
                    else sr.Write('\n');
                }
            }
            sr.Close();
            fs.Close();

            // 文件写入成功后，返回true
            return true;
        }
        #endregion

        #region 已弃用
        /// <summary>
        /// 已弃用的文件读取类！（2018.2）现采用loadCSV
        /// </summary>
        /// <returns></returns>
        public bool loadFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            string setting_path = Application.StartupPath + @"\WorkData\";
            ofd.InitialDirectory = setting_path;
            ofd.Title = "Load an excel file";
            ofd.FileName = "";
            ofd.Filter = "Excel Files(2007/2010/2013)|*.xlsx|Excel Files(2003)|*.xls";
            if (ofd.ShowDialog() == DialogResult.Cancel)
            {
                return false;
            }

            // 注意要先清空Data类，再载入新数据
            Data.clear();
            Data.ExcelFileName = ofd.FileName;
            Data.ExcelSafeFileName = ofd.SafeFileName;

            Excel.Application myExcelApp = new Excel.Application();
            Excel.Workbook myWorkBook = myExcelApp.Workbooks.Open(Data.ExcelFileName);
            Excel.Worksheet myWorkSheet = myWorkBook.ActiveSheet;
            Excel.Range range;

            Data.channelNumber = myWorkBook.Worksheets.Count;
            DataGrid[] dtArray = new DataGrid[Data.channelNumber];
            ArrayList channelLoc = new ArrayList();

            for (int i = 1; i <= Data.channelNumber; i++)
            {
                ArrayList channelData = new ArrayList();
                ArrayList channelImages = new ArrayList();
                channelLoc.Add(i);
                Data.sheetNames.Add(myWorkBook.Sheets[i].name);
                Data.rows = myWorkBook.Sheets[i].UsedRange.Rows.Count;
                Data.cols = myWorkBook.Sheets[i].UsedRange.Columns.Count;

                for (int k = 0; k < Data.rows; k++)
                {
                    ArrayList row = new ArrayList();
                    ArrayList rowImages = new ArrayList();
                    for (int j = 0; j < Data.cols; j++)
                    {
                        range = (Excel.Range)myWorkBook.Sheets[i].Cells[k + 1, j + 1];
                        row.Add(Convert.ToInt32(range.Value));
                        rowImages.Add(new PictureBox());
                    }
                    channelData.Add(row);
                    channelImages.Add(rowImages);
                }
                Data.gridData.Add(channelData);
                Data.dataImages.Add(channelImages);
            }

            Data.channel = (int[])channelLoc.ToArray(typeof(int));

            myWorkBook.Close(false);
            myExcelApp.Quit();
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(myExcelApp);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(myWorkBook);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(myWorkSheet);

            return true;
        }
        #endregion
    }
}
