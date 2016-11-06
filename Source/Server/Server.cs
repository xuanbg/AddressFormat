using System.Linq;
using System.ServiceProcess;
using Insight.Utils.AddressFormat.Entity;
using Insight.Utils.AddressFormat.Services;
using Insight.Utils.Common;
using Insight.WCF;

namespace Insight.Utils.AddressFormat
{
    public partial class Server : ServiceBase
    {
        /// <summary>
        /// 运行中的服务主机
        /// </summary>
        private static Service _Services;

        public Server()
        {
            InitializeComponent();
            Parms.BaseServer = Util.GetAppSetting("BaseServer");
            using (var context = new Entities())
            {
                Parms.Provinces = context.Region.Where(r => r.Grade == 0).ToList();
                Parms.Citys = context.Region.Where(r => r.Grade == 1).ToList();
                Parms.Countys = context.Region.Where(r => r.Grade == 2).ToList();
                Parms.Towns = context.Region.Where(r => r.Grade == 3).ToList();
            }
        }

        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            var service = new Service.Info
            {
                BaseAddress = Util.GetAppSetting("Address"),
                Port = Util.GetAppSetting("Port"),
                Path = "openapi",
                Version = Util.GetAppSetting("Version"),
                NameSpace = "Insight.WS.Utils",
                Interface = "IAfs",
                ComplyType = "Afs",
                ServiceFile = "Server.exe",
            };
            _Services = new Service();
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
