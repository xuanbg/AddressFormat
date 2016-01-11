using System;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using Insight.WS.Log.Entity;
using static Insight.WS.Log.Util;

namespace Insight.WS.Log
{
    public class General
    {

        /// <summary>
        /// 通过Session校验是否有权限访问
        /// </summary>
        /// <param name="session">用户会话</param>
        /// <param name="aid">操作ID</param>
        /// <returns></returns>
        public static JsonResult Verify(out Session session, string aid)
        {
            session = GetAuthorization<Session>();
            var result = new JsonResult();
            return session == null ? result.InvalidAuth() : Verification(session, aid);
        }

        /// <summary>
        /// 通过指定的Rule校验是否有权限访问
        /// </summary>
        /// <returns>JsonResult</returns>
        public static JsonResult Verify(string rule)
        {
            var result = new JsonResult();
            var obj = GetAuthorization<string>();
            return obj != Hash(rule) ? result.InvalidAuth() : result.Success();
        }

        /// <summary>
        /// 会话合法性验证，用于持久化客户端
        /// </summary>
        /// <param name="obj">用户会话</param>
        /// <param name="aid">操作ID</param>
        /// <returns>JsonResult</returns>
        public static JsonResult Verification(Session obj, string aid)
        {
            var url = Address + "verify/auth";
            var data = $"action={aid}";
            var author = Base64(obj);
            return HttpRequest(url, "GET", author, data);
        }

        /// <summary>
        /// 获取Authorization承载的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>数据对象</returns>
        public static T GetAuthorization<T>()
        {
            var woc = WebOperationContext.Current;
            if (woc == null) return default(T);

            var headers = woc.IncomingRequest.Headers;
            if (!CompareVersion(headers))
            {
                woc.OutgoingResponse.StatusCode = HttpStatusCode.NotAcceptable;
                return default(T);
            }

            var auth = headers[HttpRequestHeader.Authorization];
            if (string.IsNullOrEmpty(auth))
            {
                woc.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                return default(T);
            }

            var response = woc.OutgoingResponse;
            response.Headers[HttpResponseHeader.ContentEncoding] = "gzip";
            response.ContentType = "application/x-gzip";

            try
            {
                var buffer = Convert.FromBase64String(auth);
                var json = Encoding.UTF8.GetString(buffer);
                return Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                var log = new SYS_Logs
                {
                    Code = "300001",
                    Level = 3,
                    Source = "日志服务",
                    Action = "身份验证",
                    Message = ex.ToString(),
                    CreateTime = DateTime.Now
                };
                DataAccess.WriteToFile(log);
                woc.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                return default(T);
            }
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
            if (method == "GET") url += (data == "" ? "" : "?") + data;
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
            request.ContentType = "application/json";
            if (author != null)
            {
                request.Headers.Add(HttpRequestHeader.Authorization, author);
            }

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
            var accept = headers[HttpRequestHeader.Accept].Split(Convert.ToChar(";"));
            if (accept.Length < 2) return false;

            var ver = Convert.ToInt32(accept[1].Substring(9));
            return ver >= Convert.ToInt32(CompatibleVersion) && ver <= Convert.ToInt32(UpdateVersion);
        }

    }
}
