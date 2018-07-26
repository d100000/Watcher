﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Helper
{
    /// <summary>
    /// Json操作类
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// json decode 
        /// </summary>
        /// <typeparam name="T">目标类</typeparam>
        /// <param name="json">Json数据</param>
        /// <returns></returns>
        public static T Deserialize<T>(string json) where T : new()
        {
            json = json.Replace("\r\n", "");// 去除非法字段
            T t = new T();
            if (json.Contains(@"""data"":false"))// 处理错误返回信息，防止反序列化错误
            {
                json = json.Replace(@"""data"":false", @"""data"":{}");
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception Ext)
            {
                // TODO:错误处理
                return new T();
            }
        }

        /// <summary>
        /// json encode
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}
