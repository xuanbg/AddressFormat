﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Insight.WS.Log.Entity;

namespace Insight.WS.Log.Utils
{
    public class DataAccess
    {

        /// <summary>
        /// 从数据库加载日志规则到缓存
        /// </summary>
        public static void ReadRule()
        {
            using (var context = new LogEntities())
            {
                Util.Rules = context.SYS_Logs_Rules.ToList();
            }
        }

        /// <summary>
        /// 保存日志规则到数据库
        /// </summary>
        public static bool AddRule(SYS_Logs_Rules rule)
        {
            using (var context = new LogEntities())
            {
                context.SYS_Logs_Rules.Add(rule);
                if (context.SaveChanges() <= 0)
                {
                    var log = new Logger("300601");
                    log.Write();
                    return false;
                }
                Util.Rules.Add(rule);
                return true;
            }
        }

        /// <summary>
        /// 删除日志规则
        /// </summary>
        public static bool DeleteRule(Guid id)
        {
            using (var context = new LogEntities())
            {
                var rule = context.SYS_Logs_Rules.SingleOrDefault(r => r.ID == id);
                if (rule == null) return false;

                context.SYS_Logs_Rules.Remove(rule);
                if (context.SaveChanges() <= 0)
                {
                    var log = new Logger("300602");
                    log.Write();
                    return false;
                }

                Util.Rules.RemoveAll(r => r.ID == id);
                return true;
            }
        }

        /// <summary>
        /// 编辑日志规则
        /// </summary>
        public static bool EditRule(SYS_Logs_Rules rule)
        {
            using (var context = new LogEntities())
            {
                var data = context.SYS_Logs_Rules.SingleOrDefault(r => r.ID == rule.ID);
                if (data == null) return false;

                data.ToDataBase = rule.ToDataBase;
                data.Code = rule.Code;
                data.Level = rule.Level;
                data.Source = rule.Source;
                data.Action = rule.Action;
                data.Message = rule.Message;
                if (context.SaveChanges() <= 0)
                {
                    var log = new Logger("300603");
                    log.Write();
                    return false;
                }
            }

            Util.Rules.RemoveAll(r => r.ID == rule.ID);
            Util.Rules.Add(rule);
            return true;
        }

        /// <summary>
        /// 将日志写入数据库
        /// </summary>
        /// <param name="log"></param>
        /// <returns>bool 是否写入成功</returns>
        public static bool WriteToDB(SYS_Logs log)
        {
            using (var context = new LogEntities())
            {
                context.SYS_Logs.Add(log);
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// 将日志写入文件
        /// </summary>
        /// <param name="log"></param>
        /// <returns>bool 是否写入成功</returns>
        public static bool WriteToFile(SYS_Logs log)
        {
            Util.Mutex.WaitOne();
            var path = $"{Util.GetAppSetting("LogLocal")}\\{GetLevelName(log.Level)}\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path += $"{DateTime.Today.ToString("yyyy-MM-dd")}.log";
            var time = log.CreateTime.ToString("O");
            var text = $"[{log.CreateTime.Kind} {time}] [{log.Code}] [{log.Source}] [{log.Action}] Message:{log.Message}\r\n";
            var buffer = Encoding.UTF8.GetBytes(text);
            try
            {
                using (var stream = new FileStream(path, FileMode.Append))
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
                Util.Mutex.ReleaseMutex();
                return true;
            }
            catch (Exception)
            {
                Util.Mutex.ReleaseMutex();
                return false;
            }
        }

        /// <summary>
        /// 获取事件等级名称
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private static string GetLevelName(int level)
        {
            var name = (Level)level;
            return name.ToString();
        }

    }
}
