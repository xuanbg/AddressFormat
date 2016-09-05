using System.ServiceModel;
using Insight.Utils.Common;
using Insight.Utils.Server;
using Insight.WS.Utils.Entity;

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
        public JsonResult GetRegion(string address)
        {
            var verifyurl = $"{Server.BaseServer}{Server.VerifyInterface}";
            var verify = new Verify(verifyurl, 60, true);
            var result = Util.Copy<JsonResult>(verify.Result);
            if (!result.Successful) return result;

            if (string.IsNullOrEmpty(address))
            {
                result.BadRequest();
                return result;
            }

            var region = new RegionFormat().Format(address);
            if (region == null) result.IdentifyingAddressFailed();
            else result.Success(region);

            return result;
        }
    }
}
