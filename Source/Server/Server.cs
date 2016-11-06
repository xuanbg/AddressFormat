using System.Linq;
using System.ServiceProcess;
using Insight.WCF;
using Insight.WCF.Entity;
using Insight.WS.Utils.Entity;
using static Insight.Utils.Common.Util;
using static Insight.WS.Utils.Utils.Parms;

namespace Insight.WS.Utils
{
    public partial class Server : ServiceBase
    {
        /// <summary>
        /// 运行中的服务主机
        /// </summary>
        private static Services _Services;

        public Server()
        {
            InitializeComponent();
            BaseServer = GetAppSetting("BaseServer");
            using (var context = new Entities())
            {
                Provinces = context.Region.Where(r => r.Grade == 0).ToList();
                Citys = context.Region.Where(r => r.Grade == 1).ToList();
                Countys = context.Region.Where(r => r.Grade == 2).ToList();
                Towns = context.Region.Where(r => r.Grade == 3).ToList();
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
            _Services = new Services();
            _Services.CreateHost(service);
            _Services.StartService();
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        protected override void OnStop()
        {
            _Services.StopService();
        }
    }
}
