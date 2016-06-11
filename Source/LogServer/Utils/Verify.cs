using System;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using Insight.WS.Log.Entity;

namespace Insight.WS.Log.Utils
{
    public class Verify
    {
        /// <summary>
        /// 验证结果
        /// </summary>
        public JsonResult Result = new JsonResult().InvalidAuth();

        /// <summary>
        /// 用户会话对象
        /// </summary>
        public Session Session;

        /// <summary>
        /// 当前Web操作上下文
        /// </summary>
        private readonly WebOperationContext Context = WebOperationContext.Current;

        /// <summary>
        /// 通过指定的Rule校验是否有权限访问
        /// </summary>
        /// <returns>JsonResult</returns>
        public Verify(string rule)
        {
            var auth = GetAuthorization();
            var key = GetAuthor<string>(auth);
            if (key == Util.Hash(rule)) Result.Success();
        }

        /// <summary>
        /// 通过Session校验是否有权限访问
        /// </summary>
        /// <returns></returns>
        public Verify()
        {
            var url = Util.BaseServer + "verify";
            var auth = GetAuthorization();
            Result = new HttpRequest(url, auth).Result;
        }

        /// <summary>
        /// 带鉴权的会话合法性验证
        /// </summary>
        /// <param name="aid">操作ID</param>
        /// <returns>JsonResult</returns>
        public Verify(Guid aid)
        {
            var url = Util.BaseServer + $"verify/auth?action={aid}";
            var auth = GetAuthorization();
            Session = GetAuthor<Session>(auth);
            Result = new HttpRequest(url, auth).Result;
        }

        /// <summary>
        /// 获取Http请求头部承载的验证信息
        /// </summary>
        /// <returns>string Http请求头部承载的验证字符串</returns>
        private string GetAuthorization()
        {
            var headers = Context.IncomingRequest.Headers;
            var response = Context.OutgoingResponse;
            var auth = headers[HttpRequestHeader.Authorization];
            if (!string.IsNullOrEmpty(auth)) return auth;

            response.StatusCode = HttpStatusCode.Unauthorized;
            return null;
        }

        /// <summary>
        /// 获取Authorization承载的数据
        /// </summary>
        /// <param name="auth">验证信息</param>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>数据对象</returns>
        private T GetAuthor<T>(string auth)
        {
            try
            {
                var buffer = Convert.FromBase64String(auth);
                var json = Encoding.UTF8.GetString(buffer);
                return Util.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                var log = new Logger("500601", $"提取验证信息失败。\r\nException:{ex}");
                log.Write();
                return default(T);
            }
        }

    }
}
