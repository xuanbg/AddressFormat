using System;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
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
        /// <param name="source">来源（可为空）</param>
        /// <param name="action">操作（可为空）</param>
        /// <param name="userid">事件源用户ID（可为空）</param>
        /// <returns>JsonResult</returns>
        public JsonResult WriteToLog(string code, string message, string source, string action, string userid)
        {
            var result = Verify(code + Secret);
            if (!result.Successful) return result;

            var gp = new UserIdParse(userid);
            if (!gp.Successful) return result.InvalidGuid();

            var succe = WriteLog(code, message, source, action, gp.UserId);
            if (!succe.HasValue) return result.InvalidEventCode();

            return succe.Value ? result : result.DataBaseError();
        }

        /// <summary>
        /// 新增日志规则
        /// </summary>
        /// <param name="rule">日志规则数据对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult AddRule(SYS_Logs_Rules rule)
        {
            Session us;
            var result = Authorization("60A97A33-0E6E-4856-BB2B-322FEEEFD96A", out us);
            if (!result.Successful) return result;

            if (string.IsNullOrEmpty(rule.Code) || !Regex.IsMatch(rule.Code, @"^\d{6}$")) return result.InvalidEventCode();

            var level = Convert.ToInt32(rule.Code.Substring(0, 1));
            if (level <= 1 || level == 7) return result.EventWithoutConfig();

            if (Rules.Any(r => r.Code == rule.Code)) return result.EventCodeUsed();

            rule.CreatorUserId = us.UserId;
            if (!DataAccess.AddRule(rule)) return result.DataBaseError();

            var log = new
            {
                UserID = us.UserId,
                Message = $"事件代码【{rule.Code}】已由{us.UserName}创建和配置为：{Serialize(rule)}"
            };
            WriteLog("600601", Serialize(log));
            return result;
        }

        /// <summary>
        /// 删除日志规则
        /// </summary>
        /// <param name="id">日志规则ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult RemoveRule(string id)
        {
            Session us;
            var result = Authorization("BBC43098-A030-46CA-A681-0C3D1ECC15AB", out us);
            if (!result.Successful) return result;

            Guid rid;
            if (!Guid.TryParse(id, out rid)) return result.InvalidGuid();

            if (!DataAccess.DeleteRule(rid)) return result.DataBaseError();

            var log = new
            {
                UserID = us.UserId,
                Message = $"事件配置【{id}】已被{us.UserName}删除"
            };
            WriteLog("600602", Serialize(log));
            return result;
        }

        /// <summary>
        /// 编辑日志规则
        /// </summary>
        /// <param name="rule">日志规则数据对象</param>
        /// <returns>JsonResult</returns>
        public JsonResult EditRule(SYS_Logs_Rules rule)
        {
            Session us;
            var result = Authorization("9FF1547D-2E3F-4552-963F-5EA790D586EA", out us);
            if (!result.Successful) return result;

            if (!DataAccess.EditRule(rule)) return result.DataBaseError();

            var log = new
            {
                UserID = us.UserId,
                Message = $"事件代码【{rule.Code}】已被{us.UserName}修改为：{Serialize(rule)}"
            };
            WriteLog("600603", Serialize(log));
            return result;
        }

        /// <summary>
        /// 获取日志规则
        /// </summary>
        /// <param name="id">日志规则ID</param>
        /// <returns>JsonResult</returns>
        public JsonResult GetRule(string id)
        {
            var result = Authorization("E3CFC5AA-CD7D-4A3C-8900-8132ADB7099F");
            if (!result.Successful) return result;

            Guid rid;
            if (!Guid.TryParse(id, out rid)) return result.InvalidGuid();

            var rule = Rules.SingleOrDefault(r => r.ID == rid);
            return rule == null ? result.NotFound() : result.Success(Serialize(rule));
        }

        /// <summary>
        /// 获取全部日志规则
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult GetRules()
        {
            var result = Authorization("E3CFC5AA-CD7D-4A3C-8900-8132ADB7099F");
            if (!result.Successful) return result;

            return Rules.Count == 0 ? result.NoContent() : result.Success(Serialize(Rules));
        }
    }
}
