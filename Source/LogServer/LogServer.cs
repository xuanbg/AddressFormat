using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceProcess;
using System.Windows.Forms;
using Insight.WS.Base;

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
            InitVersion();
            DataAccess.ReadRule();
        }

        protected override void OnStart(string[] args)
        {
            var path = $"{Application.StartupPath}\\LogServer.exe";
            var endpoints = new List<EndpointSet> { new EndpointSet { Name = "IlogService" } };
            var serv = new Services
            {
                BaseAddress = Util.GetAppSetting("Address"),
                Port = Util.GetAppSetting("Port"),
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
        /// 读取版本信息
        /// </summary>
        public static void InitVersion()
        {
            var version = new Version(Application.ProductVersion);
            var build = $"{version.Major}{version.Minor}{version.Build.ToString("D4").Substring(0, 2)}";
            Util.Version = Convert.ToInt32(build);
        }

    }
}
