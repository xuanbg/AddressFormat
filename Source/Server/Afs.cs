using System.ServiceModel;
using Insight.Utils.Entity;
using Insight.Utils.Server;
using Insight.WS.Utils.Utils;

namespace Insight.WS.Utils
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class Afs : IAfs
    {
        /// <summary>
        /// 获取结构化地址
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns>JsonResult</returns>
        public Result GetRegion(string address)
        {
            var url = $"{Parms.BaseServer}/security/v1.0/tokens/verify";
            var verify = new Verify(url, 60, true);
            var result = verify.Result;
            if (!result.Successful) return result;

            if (string.IsNullOrEmpty(address))
            {
                result.BadRequest();
                return result;
            }

            var region = new RegionFormat().Format(address);
            result.Success(region);

            return result;
        }
    }
}
