using System.Collections;

namespace PrinterPro 
{
    public class Data
    {
        public static string ExcelFileName = "No File", ExcelSafeFileName = "No File";

        public static int channelNumber;
        public static int rows, cols;

        public static int[] channel, frequency, pulsewidth;
        public static double[] xRelative, yRelative;
        public static double xStart, yStart;
        public static double xDistance, yDistance;
        public static float idleAccn = 1000, workAccn = 20;

        public static ArrayList gridData = new ArrayList();
        public static ArrayList dataImages = new ArrayList();

        public static ArrayList sheetNames = new ArrayList();

        // Printing Preferences
        public static double idleSpeed, workSpeed;
        public static int selectedStyle = 0;
        public static int waitTime;

        public static void clear()
        {
            ExcelFileName = "No File";
            ExcelSafeFileName = "No File";

            channelNumber = 0;
            rows = 0;
            cols = 0;

            channel = null;
            frequency = null;
            pulsewidth = null;
            xRelative = null;
            yRelative = null;

            gridData = new ArrayList();
            dataImages = new ArrayList();
            sheetNames = new ArrayList();
        }
    }
}
