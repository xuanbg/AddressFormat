using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using Insight.WCF;
using Insight.WCF.Entity;
using Insight.WS.Utils.Entity;
using static Insight.Utils.Common.Util;

namespace Insight.WS.Utils
{
    public partial class Server : ServiceBase
    {
        /// <summary>
        /// 基础服务地址
        /// </summary>
        public static string BaseServer;


        public static string VerifyInterface;

        /// <summary>
        /// 运行中的服务主机
        /// </summary>
        public static Services Services;

        /// <summary>
        /// 行政区划表缓存
        /// </summary>
        public static List<Region> Regions;

        public Server()
        {
            InitializeComponent();
            BaseServer = GetAppSetting("BaseServer");
            VerifyInterface = GetAppSetting("VerifyInterface");
            using (var context = new Entities())
            {
                Regions = context.Region.Where(r => r.Grade < 4).ToList();
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            var service = new ServiceInfo
            {
                BaseAddress = GetAppSetting("Address"),
                Port = GetAppSetting("Port"),
                Path = "openapi",
                Version = GetAppSetting("Version"),
                NameSpace = "Insight.WS.Utils",
                Interface = "IAfs",
                ComplyType = "Afs",
                ServiceFile = "Server.exe",
            };
            Services = new Services();
            Services.CreateHost(service);
            Services.StartService();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        protected override void OnStop()
        {
            Services.StopService();
        }
    }
}
