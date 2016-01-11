using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceProcess;
using Microsoft.Samples.GZipEncoder;

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
        }

        protected override void OnStart(string[] args)
        {
            CreateHost();
            Host.Open();
            DataAccess.ReadRule();
        }

        protected override void OnStop()
        {
            Host.Abort();
            Host.Close();
        }

        /// <summary>
        /// 创建服务主机
        /// </summary>
        private static void CreateHost()
        {
            var transport = new HttpTransportBindingElement
            {
                ManualAddressing = true,
                MaxReceivedMessageSize = 1073741824,
                TransferMode = TransferMode.Streamed
            };
            var encoder = new WebMessageEncodingBindingElement
            {
                ReaderQuotas = { MaxArrayLength = 67108864, MaxStringContentLength = 67108864 },
                ContentTypeMapper = new TypeMapper()
            };
            var gZipEncode = new GZipMessageEncodingBindingElement(encoder);
            var binding = new CustomBinding
            {
                SendTimeout = TimeSpan.FromSeconds(600),
                ReceiveTimeout = TimeSpan.FromSeconds(600),
                Elements = { gZipEncode, transport }
            };

            var address = new Uri(Util.GetAppSetting("Address"));
            Host = new ServiceHost(typeof(LogManage), address);
            var endpoint = Host.AddServiceEndpoint(typeof(Interface), binding, "LogServer");
            endpoint.Behaviors.Add(new WebHttpBehavior());
        }

    }
}
