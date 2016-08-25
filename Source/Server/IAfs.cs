using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.WS.Utils.Entity;

namespace Insight.WS.Utils
{
    [ServiceContract]
    interface IAfs
    {
        /// <summary>
        /// 获取结构化地址
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "openapi/regions/{address}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        JsonResult GetRegion(string address);
    }
}
