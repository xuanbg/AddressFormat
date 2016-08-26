using Insight.Utils.Entity;

namespace Insight.WS.Utils.Entity
{
    /// <summary>
    /// Json接口返回值
    /// </summary>
    public class JsonResult:Result
    {
        /// <summary>
        /// 地址识别失败（413）
        /// </summary>
        /// <returns>JsonResult</returns>
        public void IdentifyingAddressFailed()
        {
            Successful = false;
            Code = "413";
            Name = "IdentifyingAddressFailed";
            Message = "地址识别失败";
        }
    }
}
