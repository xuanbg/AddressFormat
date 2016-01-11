using System;
using System.Linq;
using System.ServiceModel;
using Insight.WS.Log.Entity;
using static Insight.WS.Log.General;
using static Insight.WS.Log.Util;

namespace Insight.WS.Log
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class LogManage : Interface
    {

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="code">事件代码（必须有）</param>
        /// <param name="message">事件消息，为空则使用默认消息文本</param>
        /// <param name="userid">事件源用户ID（可为空）</param>
        /// <returns>JsonResult</returns>
        public JsonResult WriteToLog(string code, string message, string userid)
        {
            var result = Verify(code + Secret);
            if (!result.Successful) return result;

            var gp = new UserIdParse(userid);
            if (!gp.Successful) return result.InvalidGuid();

            var rule = Rules.SingleOrDefault(r => r.Code == code);
            if (rule == null) return result.InvalidEventCode();

            var log = new SYS_Logs
            {
                ID = Guid.NewGuid(),
                Code = code,
                Level = rule.Level,
                Source = rule.Source,
                Action = rule.Action,
                Message = string.IsNullOrEmpty(message) ? rule.Message : message,
                SourceUserId = gp.UserId,
                CreateTime = DateTime.Now
            };

            if (rule.ToDataBase)
            {
                DataAccess.WriteToDB(log);
            }
            else
            {
                DataAccess.WriteToFile(log);
            }
            return result;
        }

        /// <summary>
        /// 新增日志规则
        /// </summary>
        /// <param name="rule">日志规则数据对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddRule(SYS_Logs_Rules rule)
        {
            Session us;
            var result = Verify(out us, "60A97A33-0E6E-4856-BB2B-322FEEEFD96A");
            if (!result.Successful) return result;

            return DataAccess.AddRule(rule) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 删除日志规则
        /// </summary>
        /// <param name="id">日志规则ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveRule(string id)
        {
            Session us;
            var result = Verify(out us, "BBC43098-A030-46CA-A681-0C3D1ECC15AB");
            if (!result.Successful) return result;

            Guid rid;
            if (!Guid.TryParse(id, out rid)) return result.InvalidGuid();

            return DataAccess.DeleteRule(rid) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 编辑日志规则
        /// </summary>
        /// <param name="rule">日志规则数据对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult EditRule(SYS_Logs_Rules rule)
        {
            Session us;
            var result = Verify(out us, "9FF1547D-2E3F-4552-963F-5EA790D586EA");
            if (!result.Successful) return result;

            return DataAccess.EditRule(rule) ? result : result.DataBaseError();
        }

        /// <summary>
        /// 获取日志规则
        /// </summary>
        /// <param name="id">日志规则ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetRule(string id)
        {
            Session us;
            var result = Verify(out us, "E3CFC5AA-CD7D-4A3C-8900-8132ADB7099F");
            if (!result.Successful) return result;

            Guid rid;
            if (!Guid.TryParse(id, out rid)) return result.InvalidGuid();

            var rule = Rules.SingleOrDefault(r => r.ID == rid);
            return rule == null ? result.NotFound() : result.Success(Serialize(rule));
        }

    }
}
