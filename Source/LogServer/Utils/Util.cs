using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Insight.WS.Log.Entity;

namespace Insight.WS.Log.Utils
{
    public static class Util
    {

        #region 成员属性

        /// <summary>
        /// 安全码
        /// </summary>
        public const string Secret = "842A381C91CE43A98720825601C22A56";

        /// <summary>
        /// 基础服务路径
        /// </summary>
        public static string BaseServer;

        /// <summary>
        /// 日志规则列表
        /// </summary>
        public static List<SYS_Logs_Rules> Rules;

        /// <summary>
        /// 进程同步基元
        /// </summary>
        public static readonly Mutex Mutex = new Mutex();

        #endregion

        #region 常用方法

        /// <summary>
        /// 读取配置项的值
        /// </summary>
        /// <param name="key">配置项</param>
        /// <returns>配置项的值</returns>
        public static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 计算字符串的Hash值
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>String Hash值</returns>
        public static string Hash(string str)
        {
            var md5 = MD5.Create();
            var s = md5.ComputeHash(Encoding.UTF8.GetBytes(str.Trim()));
            return s.Aggregate("", (current, c) => current + c.ToString("X2"));
        }

        /// <summary>
        /// 生成用于接口验证的Authorization字符串
        /// </summary>
        /// <typeparam name="T">输入类型</typeparam>
        /// <param name="obj">用于接口验证的数据对象</param>
        /// <returns>string 用于接口验证的Authorization字符串</returns>
        public static string Base64<T>(T obj)
        {
            var json = Serialize(obj);
            var buff = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(buff);
        }

        #endregion

        #region Serialize/Deserialize

        /// <summary>
        /// 将一个对象序列化为Json字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>string Json字符串</returns>
        public static string Serialize<T>(T obj)
        {
            return new JavaScriptSerializer().Serialize(obj);
        }

        /// <summary>
        /// 将一个Json字符串反序列化为指定类型的对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">Json字符串</param>
        /// <returns>T 反序列化的对象</returns>
        public static T Deserialize<T>(string json)
        {
            return new JavaScriptSerializer().Deserialize<T>(json);
        }

        #endregion

        #region Compress/Decompress

        /// <summary>
        /// GZip压缩
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] data)
        {
            var ms = new MemoryStream();
            var stream = new GZipStream(ms, CompressionMode.Compress, true);
            stream.Write(data, 0, data.Length);
            stream.Close();
            return ms.ToArray();
        }

        /// <summary>
        /// ZIP解压
        /// </summary>
        /// <param name="dada"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] dada)
        {
            var ms = new MemoryStream(dada);
            var stream = new GZipStream(ms, CompressionMode.Decompress);
            var buffer = new MemoryStream();
            var block = new byte[1024];
            while (true)
            {
                var read = stream.Read(block, 0, block.Length);
                if (read <= 0) break;
                buffer.Write(block, 0, read);
            }
            stream.Close();
            return buffer.ToArray();
        }

        #endregion

    }
}
