﻿using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.WS.Log.Entity;

namespace Insight.WS.Log
{
    [ServiceContract]
    interface ILogService
    {
        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="code">事件代码（必须有）</param>
        /// <param name="message">事件消息，为空则使用默认消息文本</param>
        /// <param name="source">来源（可为空）</param>
        /// <param name="action">操作（可为空）</param>
        /// <param name="userid">事件源用户ID（可为空）</param>
        /// <param name="key"></param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "logs", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult WriteToLog(string code, string message, string source, string action, string userid, string key);

        /// <summary>
        /// 新增日志规则
        /// </summary>
        /// <param name="rule">日志规则数据对象</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "POST", UriTemplate = "rules", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult AddRule(SYS_Logs_Rules rule);

        /// <summary>
        /// 删除日志规则
        /// </summary>
        /// <param name="id">日志规则ID</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "DELETE", UriTemplate = "rules/{id}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult RemoveRule(string id);

        /// <summary>
        /// 编辑日志规则
        /// </summary>
        /// <param name="rule">日志规则数据对象</param>
        /// <returns>JsonResult</returns>
        [WebInvoke(Method = "PUT", UriTemplate = "rules", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [OperationContract]
        JsonResult EditRule(SYS_Logs_Rules rule);

        /// <summary>
        /// 获取日志规则
        /// </summary>
        /// <param name="id">日志规则ID</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "rules/{id}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetRule(string id);

        /// <summary>
        /// 获取全部日志规则
        /// </summary>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "rules", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetRules();

    }
}
