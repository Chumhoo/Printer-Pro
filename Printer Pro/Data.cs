using System.Collections;

namespace PrinterPro
{
    public class Data
    {
        public static string ExcelFileName, ExcelSafeFileName;

        public static int SolutionNumber;
        public static int rowCount, colCount;

        public static int[] channel, frequency, pulsewidth;
        public static double[] xRelative, yRelative;
        public static double xStart, yStart;
        public static double xDistance, yDistance;

        public static ArrayList gridData = new ArrayList();
        public static ArrayList sheetNames = new ArrayList();

        public static ArrayList allGrids = new ArrayList();

        // Printing Preferences
        public static double idleSpeed, workSpeed;
        public static int selectedStyle = 0;
        public static int waitTime;

        public static void clear()
        {
            ExcelFileName = null;
            ExcelSafeFileName = null;

            SolutionNumber = 0;
            rowCount = 0;
            colCount = 0;

            channel = null;
            frequency = null;
            pulsewidth = null;
            xRelative = null;
            yRelative = null;

            gridData = new ArrayList();
            sheetNames = new ArrayList();
            allGrids = new ArrayList();
        }
    }
}
