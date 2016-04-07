using System.ServiceProcess;
using Insight.WS.Log.Entity;
using Insight.WS.Log.Utils;
using static Insight.WS.Log.Utils.Util;

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
            var ver = GetAppSetting("Version");
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
            Services.CreateHost(service, ver);
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
            BaseServer = GetAppSetting("BaseServer");
        }

    }
}
