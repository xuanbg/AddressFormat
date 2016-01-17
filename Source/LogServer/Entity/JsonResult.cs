namespace Insight.WS.Log.Entity
{
    /// <summary>
    /// Json接口返回值
    /// </summary>
    public class JsonResult
    {
        /// <summary>
        /// 结果
        /// </summary>
        public bool Successful { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 错误名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 初始化为未知错误（500）的错误信息
        /// </summary>
        public JsonResult()
        {
            Code = "500";
            Name = "UnknownError";
            Message = "未知错误";
        }

        #region 接口返回信息

        /// <summary>
        /// 返回接口调用成功（200）的成功信息
        /// </summary>
        /// <param name="data">承载的数据（可选）</param>
        /// <returns>JsonResult</returns>
        public JsonResult Success(string data = null)
        {
            Successful = true;
            Code = "200";
            Name = "OK";
            Message = "接口调用成功";
            Data = data;
            return this;
        }

        /// <summary>
        /// 返回身份验证失败（401）的错误信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult InvalidAuth()
        {
            Successful = false;
            Code = "401";
            Name = "InvalidAuthenticationInfo";
            Message = "提供的身份验证信息不正确";
            return this;
        }

        /// <summary>
        /// 返回未找到指定资源（404）的错误信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult NotFound()
        {
            Successful = false;
            Code = "404";
            Name = "ResourceNotFound";
            Message = "指定的资源不存在";
            return this;
        }

        /// <summary>
        /// 返回Guid转换失败（406）的错误信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult InvalidGuid()
        {
            Successful = false;
            Code = "406";
            Name = "InvalidGUID";
            Message = "非法的GUID";
            return this;
        }

        /// <summary>
        /// 返回事件代码错误（413）的错误信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult InvalidEventCode()
        {
            Successful = false;
            Code = "413";
            Name = "InvalidEventCode";
            Message = "未配置的事件代码，请先为该代码配置日志规则";
            return this;
        }

        /// <summary>
        /// 返回事件代码已使用（414）的错误信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult EventCodeUsed()
        {
            Successful = false;
            Code = "414";
            Name = "EventCodeUsed";
            Message = "事件代码已使用，请勿重复为该代码配置日志规则";
            return this;
        }

        /// <summary>
        /// 返回数据库操作失败（501）的错误信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult DataBaseError()
        {
            Successful = false;
            Code = "501";
            Name = "DataBaseError";
            Message = "写入数据失败";
            return this;
        }

        #endregion

    }
}
