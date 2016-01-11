using System;

namespace Insight.WS.Log.Entity
{
    public class UserIdParse
    {

        /// <summary>
        /// 转换结果
        /// </summary>
        public bool Successful = true;

        /// <summary>
        /// 转换成功后的用户ID
        /// </summary>
        public Guid? UserId;

        /// <summary>
        /// 将一个字符串转换为可为空的GUID
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        public UserIdParse(string str)
        {
            if (string.IsNullOrEmpty(str)) return;

            Guid uid;
            Successful = Guid.TryParse(str, out uid);
            if (Successful) UserId = uid;
        }

    }
}
