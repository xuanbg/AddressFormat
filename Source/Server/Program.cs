using System.ServiceProcess;

namespace Insight.WS.Utils
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            var service = new ServiceBase[]
            {
                new Server()
            };
            ServiceBase.Run(service);
        }
    }
}
