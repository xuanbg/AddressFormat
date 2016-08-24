using Insight.Utils.Entity;

namespace Insight.WS.Utils.Entity
{
    /// <summary>
    /// Json接口返回值
    /// </summary>
    public class JsonResult:Result
    {

        #region 接口返回信息

        /// <summary>
        /// 返回事件代码错误（413）的错误信息
        /// </summary>
        /// <returns>JsonResult</returns>
        public JsonResult IdentifyingAddressFailed()
        {
            Successful = false;
            Code = "413";
            Name = "IdentifyingAddressFailed";
            Message = "地址识别失败";
            return this;
        }

        #endregion

    }
}
