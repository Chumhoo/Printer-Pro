using System;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms;
using System.Collections;

namespace PrinterPro
{
    class FileLoader
    {
        public void load(string FileName, string SafeFileName)
        {
            Data.clear();
            Data.ExcelFileName = FileName;
            Data.ExcelSafeFileName = SafeFileName;

            Excel.Application myExcelApp = new Excel.Application();
            Excel.Workbook myWorkBook = myExcelApp.Workbooks.Open(FileName);
            Excel.Worksheet myWorkSheet = myWorkBook.ActiveSheet;
            Excel.Range range;

            Data.SolutionNumber = myWorkBook.Worksheets.Count;
            DataGrid[] dtArray = new DataGrid[Data.SolutionNumber];
            ArrayList channelLoc = new ArrayList();
            ArrayList frequencyLoc = new ArrayList();
            ArrayList pulseWidthLoc = new ArrayList();
            ArrayList xRelativeLoc = new ArrayList();
            ArrayList yRelativeLoc = new ArrayList();

            for (int i = 1; i <= Data.SolutionNumber; i++)
            {
                ArrayList pageData = new ArrayList();
                channelLoc.Add(i);
                frequencyLoc.Add(10);
                pulseWidthLoc.Add(2);
                xRelativeLoc.Add((double)Convert.ToDouble((i - 1) * 5));
                yRelativeLoc.Add((double)Convert.ToDouble(0));
                Data.sheetNames.Add(myWorkBook.Sheets[i].name);
                Data.rowCount = myWorkBook.Sheets[i].UsedRange.Rows.Count;
                Data.colCount = myWorkBook.Sheets[i].UsedRange.Columns.Count;

                for (int k = 0; k < Data.rowCount; k++)
                {
                    ArrayList row = new ArrayList();
                    for (int j = 0; j < Data.colCount; j++)
                    {
                        range = (Excel.Range)myWorkBook.Sheets[i].Cells[k + 1, j + 1];
                        row.Add(Convert.ToInt32(range.Value));
                    }
                    pageData.Add(row);
                }
                Data.gridData.Add(pageData);
            }

            Data.channel = (int[])channelLoc.ToArray(typeof(int));
            Data.frequency = (int[])frequencyLoc.ToArray(typeof(int));
            Data.pulsewidth = (int[])pulseWidthLoc.ToArray(typeof(int));
            Data.xRelative = (double[])xRelativeLoc.ToArray(typeof(double));
            Data.yRelative = (double[])yRelativeLoc.ToArray(typeof(double));

            myWorkBook.Close(false);
            myExcelApp.Quit();
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(myExcelApp);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(myWorkBook);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(myWorkSheet);
        }
    }
}
