using System;
using System.Linq;
using System.Text;

namespace Helper
{
    /// <summary>
    /// 字符串编码类
    /// </summary>
    public static class EncryptHelper
    {

        /// <summary>
        /// 字符串加密
        /// </summary>
        /// <param name="passWord"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string EncryptEncode(string passWord, string data)
        {
            var pwKey = passWord.ToArray().Aggregate(0, (current, c) => current + c);

            var pdData = data.ToArray().Aggregate("", (current, c) => current + ((c + pwKey) + ","));

            return Convert.ToBase64String(Encoding.Default.GetBytes(pdData)).Replace("O", ",").Replace("E", ".").Replace(",", "OE").Replace(".", "O");
        }

        /// <summary>
        /// 字符串解密
        /// </summary>
        /// <param name="passWord"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string EncryptDecode(string passWord, string data)
        {
            var pwKey = passWord.ToArray().Aggregate(0, (current, c) => current + c);

            var sArray = DecodeBase64(data.Replace("OE", ",").Replace("O", ".").Replace(".", "E").Replace(",", "O")).Split(',');

            return sArray.Where(stringData => stringData != "").Aggregate("", (current, stringData) => current + (char) (Convert.ToInt32(stringData) - pwKey));
        }

        /// <summary>
        /// Base64 decode
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string DecodeBase64(string result)
        {
            var encode = Encoding.UTF8;
            string decode;
            var bytes = Convert.FromBase64String(result);
            try
            {
                decode = encode.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }


    }
}
