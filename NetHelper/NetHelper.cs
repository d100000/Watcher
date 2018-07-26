using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{

    /// <summary>
    /// http 操作类，通用的http操作类
    /// </summary>
    public static class NetHelper
    {
        static NetHelper()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;
        }

        public static string HttpCall(string url, string postData, HttpEnum method)
        {
            System.GC.Collect();
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            myHttpWebRequest.Timeout = 10000;
            if (method == HttpEnum.Post)
            {
                myHttpWebRequest.Method = "POST";
                //采用UTF8编码
                UTF8Encoding encoding = new UTF8Encoding();
                byte[] byte1 = encoding.GetBytes(postData);
                myHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
                myHttpWebRequest.ContentLength = byte1.Length;

                /*
                * 写请求体数据
                */

                Stream newStream = myHttpWebRequest.GetRequestStream();
                newStream.Write(byte1, 0, byte1.Length);
                newStream.Close();
            }
            else// get
                myHttpWebRequest.Method = "GET";
            myHttpWebRequest.ProtocolVersion = new Version(1, 1);   //Http/1.1版本

            string lcHtml = string.Empty;
            try
            {
                //发送成功后接收返回的JSON信息
                HttpWebResponse response = (HttpWebResponse) myHttpWebRequest.GetResponse();
                Encoding enc = Encoding.GetEncoding("UTF-8");
                Stream stream = response.GetResponseStream();
                if (stream != null)
                {
                    StreamReader streamReader = new StreamReader(stream, enc);
                    lcHtml = streamReader.ReadToEnd();
                }
                response.Close();
                response = null;
            }
            catch (Exception Ext)
            {
                myHttpWebRequest = null;
            }
            finally
            {
                myHttpWebRequest = null;

            }
            return (lcHtml);
        }

        public static string GETProperties<T>(T t)
        {
            string tStr = string.Empty;
            if (t == null)
            {
                return tStr;
            }
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (properties.Length <= 0)
            {
                return tStr;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                string name = item.Name;
                object value = item.GetValue(t, null);
                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    if (value==null)
                    {
                        continue;
                    }
                    tStr += $"{name}={value}&";
                    
                }
                else
                {
                    GETProperties(value);
                }
            }
            tStr = tStr.Substring(0, tStr.Length - 1);
            return tStr;
        }
    }
}
