using System.Collections.Generic;
using Insight.WS.Utils.Entity;

namespace Insight.WS.Utils.Utils
{
    public class Parms
    {
        /// <summary>
        /// 基础服务地址
        /// </summary>
        public static string BaseServer;

        /// <summary>
        /// 省级区划表缓存
        /// </summary>
        public static List<Region> Provinces;

        /// <summary>
        /// 市级区划表缓存
        /// </summary>
        public static List<Region> Citys;

        /// <summary>
        /// 县级区划表缓存
        /// </summary>
        public static List<Region> Countys;

        /// <summary>
        /// 镇级区划表缓存
        /// </summary>
        public static List<Region> Towns;
    }
}
