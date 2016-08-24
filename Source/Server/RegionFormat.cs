using System;
using System.Linq;
using System.Text.RegularExpressions;
using Insight.WS.Utils.Entity;
using static Insight.WS.Utils.Server;

namespace Insight.WS.Utils
{
    public class RegionFormat
    {
        private Region Province;
        private Region City;
        private Region County;
        private Region Town;
        private string Address;
        private int Ps;
        private int Cs;
        private int Position;

        /// <summary>
        /// 格式化地址为：省、市、县和街道详细地址
        /// </summary>
        /// <param name="address">地址字符串</param>
        /// <returns>Address 结构化地址数据</returns>
        public Address Format(string address)
        {
            // 清洗地址字符串中的无用字符
            var str = Regex.Replace(address, @"\s|\[|\]|^\d+", "");
            Address = Regex.Replace(str, @"^中国", "");
            if (Address.Length < 5) return SetRegion();

            // 尝试匹配省级区划
            FindProvinc:
            Province = FindProvinc();

            // 根据省级区划正查市级区划或尝试通过市级区划反查省级区划
            FindCity:
            if (Province == null) FindProvinceReverse();
            else FindCity();

            // 未匹配到省级区划，尝试从县级区划反查省市并返回结构化地址数据
            if (Province == null) return ReverseFromCounty();

            // 根据市级区划正查县级区划或尝试通过县级区划反查市级区划
            if (City == null) FindCityReverse();
            else FindCounty();

            // 如再次匹配到省级区划，则从新位置开始尝试重新匹配
            var index = Address.IndexOf(Province.Alias, Ps, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                Ps = index;
                goto FindProvinc;
            }

            // 如未匹配到市级区划，尝试从镇级区划反查县市并返回结构化地址数据
            if (City == null) return ReverseFromTown();

            // 如再次匹配到市级区划，则从新位置开始尝试重新匹配
            index = Address.IndexOf(City.Alias, Cs, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                Cs = index + City.Alias.Length;
                goto FindCity;
            }

            // 根据县级区划正查级镇区划或尝试通过镇级区划反查县级区划
            if (County == null) FindCountyReverse();
            else FindTown();

            return SetRegion();
        }

        /// <summary>
        /// 查找地址中的省级区划(前4字符)
        /// </summary>
        /// <returns>bool 是否找到省级区划</returns>
        private Region FindProvinc()
        {
            var list = (from p in Regions.Where(r => r.Grade == 0)
                        let len = Address.Length - Ps
                        let count = p.Alias.Length + 4 > Address.Length - Ps ? Address.Length - Ps : p.Alias.Length + 4
                        let index = Address.IndexOf(p.Alias, Ps, count, StringComparison.OrdinalIgnoreCase)
                        where index >= 0
                        orderby index
                        select new {region = p, index}).ToList();
            if (!list.Any()) return null;

            var prov = list.First();
            Ps = prov.index + prov.region.Alias.Length;
            return prov.region;
        }

        /// <summary>
        /// 根据市级区划ID反向查询省级区划
        /// </summary>
        private void FindProvinceReverse()
        {
            var list = (from c in Regions.Where(r => r.Grade == 1)
                        let alias = c.Alias.Split(',').FirstOrDefault(Address.Contains)
                        let key = alias ?? c.Name
                        let count = key.Length + 4 > Address.Length ? Address.Length : key.Length + 4
                        let index = Address.IndexOf(key, 0, count, StringComparison.OrdinalIgnoreCase)
                        where index >= 0
                        orderby index
                        select new {region = c, index}).ToList();
            if (!list.Any()) return;

            var city = list.First();
            City = city.region;
            Province = Regions.Single(p => p.ID == City.ParentId);
            Position = city.index;
        }

        /// <summary>
        /// 根据省级区划ID正向查询市级区划
        /// </summary>
        private void FindCity()
        {
            var p = Ps < Cs ? Cs : Ps;
            var list = (from c in Regions.Where(r => r.ParentId == Province.ID)
                        let alias = c.Alias.Split(',').FirstOrDefault(Address.Contains)
                        let key = alias ?? c.Name
                        let len = Province.Name.Length + key.Length
                        let count = len > Address.Length - p ? Address.Length - p : len
                        let index = Address.IndexOf(key, p, count, StringComparison.OrdinalIgnoreCase)
                        where index >= 0
                        orderby index
                        select new {region = c, index, key}).ToList();
            if (!list.Any()) return;

            var city = list.First();
            City = city.region;
            Position = city.index;
            Cs = city.index + city.key.Length;
        }

        /// <summary>
        /// 在省级区划范围内尝试通过县级区划反向查询市级区划
        /// </summary>
        private void FindCityReverse()
        {
            var list = (from r in Regions.Where(r => r.ParentId == Province.ID)
                        join c in Regions on r.ID equals c.ParentId
                        let alias = c.Alias.Split(',').FirstOrDefault(Address.Contains)
                        let key = alias ?? c.Name
                        let len = r.Name.Length + key.Length
                        let count = len > Address.Length - Ps ? Address.Length - Ps : len
                        let index = Address.IndexOf(key, Ps, count, StringComparison.OrdinalIgnoreCase)
                        where index >= 0
                        orderby index
                        select new {region = c, index}).ToList();
            if (!list.Any()) return;

            var county = list.First();
            County = county.region;
            City = Regions.Single(c => c.ID == County.ParentId);
            Position = county.index;
        }

        /// <summary>
        /// 根据市级区划ID正向查询县级区划
        /// </summary>
        private void FindCounty()
        {
            var list = (from c in Regions.Where(r => r.ParentId == City.ID)
                        let alias = c.Alias.Split(',').FirstOrDefault(Address.Contains)
                        let key = alias ?? c.Name
                        let len = City.Name.Length + key.Length
                        let count = len > Address.Length - Position ? Address.Length - Position : len
                        let index = Address.IndexOf(key, Position + 1, count - 1, StringComparison.OrdinalIgnoreCase)
                        where index >= 0
                        orderby index
                        select new {region = c, index}).ToList();
            if (!list.Any()) return;

            var county = list.First();
            County = county.region;
            Position = county.index;
        }

        /// <summary>
        /// 在市级区划范围内尝试通过镇级区划反向查询县级区划
        /// </summary>
        private void FindCountyReverse()
        {
            var list = (from c in Regions.Where(r => r.ParentId == City.ID)
                        join t in Regions on c.ID equals t.ParentId
                        let alias = t.Alias.Split(',').FirstOrDefault(Address.Contains)
                        let key = alias ?? t.Name
                        let len = c.Name.Length + key.Length
                        let count = len > Address.Length - Position ? Address.Length - Position : len
                        let index = Address.IndexOf(key, Position + 1, count - 1, StringComparison.OrdinalIgnoreCase)
                        where index >= 0
                        orderby index
                        select new {region = t, index}).ToList();
            if (!list.Any()) return;

            var town = list.First();
            Town = town.region;
            County = Regions.Single(c => c.ID == Town.ParentId);
            Position = town.index;
        }

        /// <summary>
        /// 根据县级区划ID正向查询镇级区划
        /// </summary>
        private void FindTown()
        {
            var list = (from t in Regions.Where(r => r.ParentId == County.ID)
                        let alias = t.Alias.Split(',').FirstOrDefault(Address.Contains)
                        let key = alias ?? t.Name
                        let len = County.Name.Length + key.Length
                        let count = len > Address.Length - Position ? Address.Length - Position : len
                        let index = Address.IndexOf(key, Position + 1, count - 1, StringComparison.OrdinalIgnoreCase)
                        where index >= 0
                        orderby index
                        select new {region = t, index}).ToList();
            if (!list.Any()) return;

            var town = list.First();
            Town = town.region;
            Position = town.index;
        }

        /// <summary>
        /// 通过县级行政区划反查省市
        /// </summary>
        /// <returns>Address 结构化地址数据</returns>
        private Address ReverseFromCounty()
        {
            var list = (from c in Regions.Where(r => r.Grade == 2)
                        let alias = c.Alias.Split(',').FirstOrDefault(Address.Contains)
                        let key = alias ?? c.Name
                        let count = key.Length + 1 > Address.Length ? Address.Length : key.Length + 1
                        let index = Address.IndexOf(key, 0, count, StringComparison.OrdinalIgnoreCase)
                        where index >= 0
                        orderby index
                        select new {region = c, index, key}).ToList();
            if (!list.Any()) return null;

            var keys = list.GroupBy(r => r.key);
            if (list.Count == 1 || keys.Count() > 1)
            {
                var county = list.First();
                County = county.region;
                Position = county.index;
                FindTown();
            }
            else
            {
                var towns = (from c in list
                             join t in Regions on c.region.ID equals t.ParentId
                             let alias = t.Alias.Split(',').FirstOrDefault(Address.Contains)
                             let key = alias ?? t.Name
                             let len = c.region.Name.Length + key.Length
                             let count = len > Address.Length - Position ? Address.Length - Position : len
                             let index = Address.IndexOf(key, Position + 1, count - 1, StringComparison.OrdinalIgnoreCase)
                             where index >= 0
                             orderby index
                             select new {region = t, index, key}).ToList();
                if (!towns.Any()) return SetRegion();

                keys = towns.GroupBy(r => r.key);
                if (towns.Count > 1 && keys.Count() == 1) return SetRegion();

                var town = towns.First();
                Town = town.region;
                County = Regions.Single(c => c.ID == Town.ParentId);
                Position = town.index;
            }

            City = Regions.Single(c => c.ID == County.ParentId);
            Province = Regions.Single(p => p.ID == City.ParentId);
            return SetRegion();
        }

        /// <summary>
        /// 通过镇级行政区划反查县市。如未能查到且省级区划为直辖市，则市级区划设置为市辖区
        /// </summary>
        /// <returns></returns>
        private Address ReverseFromTown()
        {
            var list = (from c in Regions.Where(r => r.ParentId == Province.ID)
                        join t in Regions on c.ID equals t.ParentId
                        let alias = t.Alias.Split(',').FirstOrDefault(Address.Contains)
                        let key = alias ?? t.Name
                        let count = key.Length + 1 > Address.Length ? Address.Length : key.Length + 1
                        let index = Address.IndexOf(key, 0, count, StringComparison.OrdinalIgnoreCase)
                        where index >= 0
                        orderby index
                        select new {region = t, index, key}).ToList();
            if (list.Any())
            {
                var keys = list.GroupBy(r => r.key);
                if (list.Count == 1 || keys.Count() > 1)
                {
                    var town = list.First();
                    Town = town.region;
                    County = Regions.Single(c => c.ID == Town.ParentId);
                    City = Regions.Single(c => c.ID == County.ParentId);
                    Position = town.index;
                    return SetRegion();
                }
            }

            if ("北京,天津,上海,重庆".Contains(Province.Alias))
            {
                City = Regions.Single(c => c.ParentId == Province.ID && c.Name == "市辖区");
            }
            return SetRegion();
        }

        /// <summary>
        /// 构造结构化地址数据并返回
        /// </summary>
        /// <returns>Address 结构化地址数据</returns>
        private Address SetRegion()
        {
            var region = new Address
            {
                Province = Province?.Name,
                City = City?.Name,
                County = County?.Name,
                Town = Town?.Name,
                Street = Address.Substring(Position)
            };
            return region;
        }
    }
}
