namespace Insight.WS.Utils.Entity
{
    public class Address
    {
        /// <summary>
        /// 省级区划名称
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 市级区划名称
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 县级区划名称
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// 镇级区划名称
        /// </summary>
        public string Town { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string Street { get; set; }
    }
}
