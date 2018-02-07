using System.Collections;

namespace PrinterPro 
{
    /// <summary>
    /// 数据存储静态类
    /// </summary>
    public class Data
    {
        #region 数据存储变量
        // Excel文件名
        public static string ExcelFileName = "No File", ExcelSafeFileName = "No File";
        public static int channelNumber; // channel数量（暂未启用）
        public static int rows, cols;    // 液滴行列数

        public static int[] channel, frequency, pulsewidth;  // 频率，波宽等参数
        public static double[] xRelative, yRelative;         // x,y距起点的相对位移
        public static double xStart, yStart;                 // x,y起点位置
        public static double xDistance, yDistance;           // x,y液滴间距
        public static double idleSpeed, workSpeed;           // 闲置和工作运动速度
        public static float idleAccn = 1000, workAccn = 100; // 闲置和工作运动加速度

        public static ArrayList gridData = new ArrayList();  // 表格数据
        public static ArrayList dataImages = new ArrayList();// 液滴预览图片数组
        public static ArrayList sheetNames = new ArrayList();// 文件sheet名称数组
        
        public static int selectedStyle = 0;        // 液滴打印模式
        public static int waitTime;                 // 液滴打印间隔时间
        #endregion

        /// <summary>
        /// 刷新并清空所有数据存储变量
        /// </summary>
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
