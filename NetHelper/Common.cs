using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;

namespace Helper
{
    /// <summary>
    /// http 通用的系统方法
    /// - 获取时间戳：GetTimeSecond
    /// - 由时间戳转为系统时间：ReturnDateTime
    /// - SHA1 加密：Sha1Sign
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeSecond(DateTime dataTime)
        {
            return ((dataTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString();
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp(DateTime dataTime)
        {
            return ((dataTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp()
        {
            return ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
        }

        /// <summary>
        /// 由时间戳到系统时间
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime ReturnDateTime(string date)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(date + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        /// <summary>
        /// 通过datetime格式获取 YMD格式数字类型日期
        /// 
        /// </summary>
        /// <param name="dataTime"></param>
        /// <returns></returns>
        public static long GetDateInt(DateTime dataTime)
        {
            var dateLong= dataTime.ToString("yyyyMMdd");
            return Convert.ToInt64(dateLong);
        }
        /// <summary>
        /// 通过datetime格式获取 YMD格式数字类型日期
        /// 
        /// </summary>
        /// <param name="dataTime"></param>
        /// <returns></returns>
        public static long GetDateInt()
        {
            var dateLong = DateTime.Now.ToString("yyyyMMdd");
            return Convert.ToInt64(dateLong);
        }

        /// <summary>
        /// SHA1加密==PHP（SHA1）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Sha1Sign(string data)
        {
            byte[] temp1 = Encoding.UTF8.GetBytes(data);
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            byte[] temp2 = sha.ComputeHash(temp1);
            sha.Clear();
            // 注意， 不能用这个
            //string output = Convert.ToBase64String(temp2);
            var output = BitConverter.ToString(temp2);
            output = output.Replace("-", "");
            output = output.ToLower();
            return output;
        }

        /// <summary>
        /// 获取绝对随机数
        /// </summary>
        /// <returns></returns>
        public static string GetRandOnlyId()
        {
             var timeStamp= (DateTime.Now.ToUniversalTime().Ticks - 13560192000000000) / 10000000;// 减少时间戳位数形成新的短时间戳
            var beginRand= IntToi64(new Random(GetRandomSeed()).Next(0, 99999999));
            var endRand= IntToi64(new Random(GetGuidSeed()).Next(0, 99999999));
            var randString = beginRand+ IntToi64(timeStamp)+ endRand;
            return randString;
        }

        /// <summary>
        /// 获取不重复的随机数种子
        /// system.Security.Cryptography.RNGCryptoServiceProvider的类，它采用系统当前的硬件信息、进程信息、线程信息、系统启动时间和当前精确时间作为填充因子，通过更好的算法生成高质量的随机数
        /// </summary>
        /// <returns></returns>
        static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);

        }

        /// <summary>
        /// 通过Guid  获取随机种子
        /// </summary>
        /// <returns></returns>
        static int GetGuidSeed()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            int iSeed = BitConverter.ToInt32(buffer, 0);
            return iSeed;
        }

        /// <summary>
        /// 十进制转64进制
        /// </summary>
        /// <param name="xx"></param>
        /// <returns></returns>
        public static string IntToi64(long xx)
        {
            string retStr = "";
            while (xx >= 1)
            {
                int index = Convert.ToInt16(xx - (xx / 64) * 64);
                retStr = Base64Code[index] + retStr;
                xx = xx / 64;
            }
            return retStr;
        }

        /// <summary>
        /// 64 位转化参数
        /// </summary>
        private static Dictionary<int, string> Base64Code = new Dictionary<int, string>() {
            {   0  ,"A"}, {   1  ,"B"}, {   2  ,"C"}, {   3  ,"D"}, {   4  ,"E"}, {   5  ,"F"}, {   6  ,"G"}, {   7  ,"H"}, {   8  ,"I"}, {   9  ,"J"},
            {   10  ,"K"}, {   11  ,"L"}, {   12  ,"M"}, {   13  ,"N"}, {   14  ,"O"}, {   15  ,"P"}, {   16  ,"Q"}, {   17  ,"R"}, {   18  ,"S"}, {   19  ,"T"},
            {   20  ,"U"}, {   21  ,"V"}, {   22  ,"W"}, {   23  ,"X"}, {   24  ,"Y"}, {   25  ,"Z"}, {   26  ,"a"}, {   27  ,"b"}, {   28  ,"c"}, {   29  ,"d"},
            {   30  ,"e"}, {   31  ,"f"}, {   32  ,"g"}, {   33  ,"h"}, {   34  ,"i"}, {   35  ,"j"}, {   36  ,"k"}, {   37  ,"l"}, {   38  ,"m"}, {   39  ,"n"},
            {   40  ,"o"}, {   41  ,"p"}, {   42  ,"q"}, {   43  ,"r"}, {   44  ,"s"}, {   45  ,"t"}, {   46  ,"u"}, {   47  ,"v"}, {   48  ,"w"}, {   49  ,"x"},
            {   50  ,"y"}, {   51  ,"z"}, {   52  ,"0"}, {   53  ,"1"}, {   54  ,"2"}, {   55  ,"3"}, {   56  ,"4"}, {   57  ,"5"}, {   58  ,"6"}, {   59  ,"7"},
            {   60  ,"8"}, {   61  ,"9"}, {   62  ,"+"}, {   63  ,"/"}, };


    }

    /// <summary>
    /// Http 操作
    /// </summary>
    public enum HttpEnum 
    {
        /// <summary>
        /// http post 方法
        /// </summary>
        Post=1,
        /// <summary>
        /// http get 方法
        /// </summary>
        Get=2,
        /// <summary>
        /// http put 方法
        /// </summary>
        Put=3,
        /// <summary>
        /// http delete 方法
        /// </summary>
        Delete=4
    }

}
