using System.ServiceProcess;

namespace Insight.WS.Log
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            var ServicesToRun = new ServiceBase[]
            {
                new LogServer()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
