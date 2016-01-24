using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceProcess;
using System.Windows.Forms;
using Insight.WS.Base;
using static Insight.WS.Log.Util;

namespace Insight.WS.Log
{
    public partial class LogServer : ServiceBase
    {

        /// <summary>
        /// 运行中的服务主机
        /// </summary>
        public static ServiceHost Host;

        public LogServer()
        {
            InitializeComponent();
            InitSeting();
            DataAccess.ReadRule();
        }

        protected override void OnStart(string[] args)
        {
            var path = $"{Application.StartupPath}\\LogServer.exe";
            var endpoints = new List<EndpointSet> { new EndpointSet { Name = "ILogService" } };
            var serv = new Services
            {
                BaseAddress = GetAppSetting("Address"),
                Port = GetAppSetting("Port"),
                NameSpace = "Insight.WS.Log",
                ServiceType = "LogService",
                Endpoints = endpoints
            };
            Host = serv.CreateHost(path);
            Host.Open();
        }

        protected override void OnStop()
        {
            Host.Abort();
            Host.Close();
        }

        /// <summary>
        /// 初始化环境变量
        /// </summary>
        public static void InitSeting()
        {
            var version = new Version(Application.ProductVersion);
            var build = $"{version.Major}{version.Minor}{version.Build.ToString("D4").Substring(0, 2)}";
            CurrentVersion = Convert.ToInt32(build);
            CurrentVersion = Convert.ToInt32(build);
            CompatibleVersion = GetAppSetting("CompatibleVersion");
            UpdateVersion = GetAppSetting("UpdateVersion");

            VerifyServer = GetAppSetting("VerifyServer");
        }

    }
}
