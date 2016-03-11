using System;
using System.ServiceProcess;
using System.Windows.Forms;
using Insight.WS.Service;
using static Insight.WS.Log.Util;

namespace Insight.WS.Log
{
    public partial class LogServer : ServiceBase
    {

        /// <summary>
        /// 运行中的服务主机
        /// </summary>
        public static Services Services;

        public LogServer()
        {
            InitializeComponent();
            InitSeting();
            DataAccess.ReadRule();
        }

        protected override void OnStart(string[] args)
        {
            var service = new ServiceInfo
            {
                BaseAddress = GetAppSetting("Address"),
                Port = GetAppSetting("Port"),
                NameSpace = "Insight.WS.Log",
                Interface = "ILogService",
                ComplyType = "LogService",
                ServiceFile = "LogServer.exe",
            };
            Services = new Services();
            Services.CreateHost(service);
            Services.StartService();
        }

        protected override void OnStop()
        {
            Services.StopService();
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

            BaseServer = GetAppSetting("BaseServer");
        }

    }
}
