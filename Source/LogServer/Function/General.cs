using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using Insight.WS.Log.Entity;
using static Insight.WS.Log.Util;

namespace Insight.WS.Log
{
    public class General
    {

        /// <summary>
        /// 通过指定的Rule校验是否有权限访问
        /// </summary>
        /// <returns>JsonResult</returns>
        public static JsonResult Verify(string rule)
        {
            var result = new JsonResult();
            var auth = GetAuthorization();
            var obj = GetAuthor<string>(auth);
            return obj != Hash(rule) ? result.InvalidAuth() : result.Success();
        }

        /// <summary>
        /// 通过Session校验是否有权限访问
        /// </summary>
        /// <returns></returns>
        public static JsonResult Verify()
        {
            var url = Address + "verify";
            var auth = GetAuthorization();
            return HttpRequest(url, "GET", auth);
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="aid">操作ID</param>
        /// <returns>JsonResult</returns>
        public static JsonResult Authorization(string aid)
        {
            var url = Address + "verify/auth?action={aid}";
            var auth = GetAuthorization();
            return HttpRequest(url, "GET", auth);
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="aid">操作ID</param>
        /// <param name="session">从Http请求中获取的Session</param>
        /// <returns>JsonResult</returns>
        public static JsonResult Authorization(string aid, out Session session)
        {
            session = null;
            var url = Address + "verify/auth?action={aid}";
            var auth = GetAuthorization();
            var result = HttpRequest(url, "GET", auth);
            if (!result.Successful) return result;

            session = GetAuthor<Session>(auth);
            return result;
        }

        /// <summary>
        /// 获取Http请求头部承载的验证信息
        /// </summary>
        /// <returns>string Http请求头部承载的验证字符串</returns>
        public static string GetAuthorization()
        {
            var context = WebOperationContext.Current;
            if (context == null) return null;

            var headers = context.IncomingRequest.Headers;
            if (!CompareVersion(headers))
            {
                context.OutgoingResponse.StatusCode = HttpStatusCode.NotAcceptable;
                return null;
            }

            var auth = headers[HttpRequestHeader.Authorization];
            if (string.IsNullOrEmpty(auth))
            {
                context.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                return null;
            }

            var response = context.OutgoingResponse;
            response.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
            response.ContentType = "application/x-gzip";
            return auth;
        }

        /// <summary>
        /// 获取Authorization承载的数据
        /// </summary>
        /// <param name="auth">验证信息</param>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>数据对象</returns>
        public static T GetAuthor<T>(string auth)
        {
            try
            {
                var buffer = Convert.FromBase64String(auth);
                var json = Encoding.UTF8.GetString(buffer);
                return Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                WriteLog("300601", $"提取验证信息失败。\r\nException:{ex}");
                return default(T);
            }
        }

        /// <summary>
        /// 构造SYS_Logs数据并写入
        /// </summary>
        /// <param name="code">事件代码</param>
        /// <param name="message">事件消息。可为空</param>
        /// <param name="source">事件源。可为空，默认为空</param>
        /// <param name="action">操作。可为空，默认为空</param>
        /// <param name="id">用户ID。可为空，默认为空</param>
        /// <returns>bool 是否写入成功</returns>
        public static bool? WriteLog(string code, string message = null, string source = null, string action = null, Guid? id = null)
        {
            if (string.IsNullOrEmpty(code) || !Regex.IsMatch(code, @"/d{6}")) return null;

            var level = Convert.ToInt32(code.Substring(0, 1));
            var rule = Rules.SingleOrDefault(r => r.Code == code);
            if (level > 1 && level < 7 && rule == null) return null;

            var log = new SYS_Logs
            {
                ID = Guid.NewGuid(),
                Code = code,
                Level = level,
                Source = rule?.Source ?? source,
                Action = rule?.Action ?? action,
                Message = string.IsNullOrEmpty(message) ? rule?.Message : message,
                SourceUserId = id,
                CreateTime = DateTime.Now
            };
            return (rule?.ToDataBase ?? false) ? DataAccess.WriteToDB(log) : DataAccess.WriteToFile(log);
        }

        /// <summary>
        /// HttpRequest方法
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="method">请求的方法：GET,PUT,POST,DELETE</param>
        /// <param name="author">接口认证数据</param>
        /// <param name="data">接口参数</param>
        /// <returns>JsonResult</returns>
        public static JsonResult HttpRequest(string url, string method, string author, string data = "")
        {
            var request = GetWebRequest(url, method, author);
            if (method == "GET") return GetResponse(request);

            var buffer = Encoding.UTF8.GetBytes(data);
            request.ContentLength = buffer.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(buffer, 0, buffer.Length);
            }

            return GetResponse(request);
        }

        /// <summary>
        /// 获取WebRequest对象
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="method">请求的方法：GET,PUT,POST,DELETE</param>
        /// <param name="author">接口认证数据</param>
        /// <returns>HttpWebRequest</returns>
        private static HttpWebRequest GetWebRequest(string url, string method, string author)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Accept = $"application/x-gzip/json; version={Util.Version}; client=5";
            request.ContentType = "application/json";
            request.Headers.Add(HttpRequestHeader.Authorization, author);
            return request;
        }

        /// <summary>
        /// 获取Request响应数据
        /// </summary>
        /// <param name="request">WebRequest</param>
        /// <returns>JsonResult</returns>
        private static JsonResult GetResponse(WebRequest request)
        {
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                if (responseStream == null) return null;

                using (var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")))
                {
                    var result = reader.ReadToEnd();
                    responseStream.Close();
                    return Deserialize<JsonResult>(result);
                }
            }
            catch (Exception ex)
            {
                var result = new JsonResult
                {
                    Code = "400",
                    Name = "BadRequest",
                    Message = ex.ToString()
                };
                return result;
            }
        }

        /// <summary>
        /// 验证版本是否兼容
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private static bool CompareVersion(WebHeaderCollection headers)
        {
            var accept = headers[HttpRequestHeader.Accept];
            if (accept == null) return false;

            var val = accept.Split(Convert.ToChar(";"));
            if (accept.Length < 2) return false;

            var ver = Convert.ToInt32(val[1].Substring(9));
            return ver >= Convert.ToInt32(CompatibleVersion) && ver <= Convert.ToInt32(UpdateVersion);
        }

    }
}
