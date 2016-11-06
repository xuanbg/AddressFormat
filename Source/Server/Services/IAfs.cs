using System.ServiceModel;
using System.ServiceModel.Web;
using Insight.Utils.Entity;

namespace Insight.Utils.AddressFormat.Services
{
    [ServiceContract]
    interface IAfs
    {
        /// <summary>
        /// 获取结构化地址
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns>JsonResult</returns>
        [WebGet(UriTemplate = "regions/{address}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        Result GetRegion(string address);
    }
}
