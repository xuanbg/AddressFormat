using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Insight.Utils.AddressFormat.Entity;
using Insight.WS.Utils.Entity;
using static Insight.Utils.AddressFormat.Services.Parms;

namespace Insight.Utils.AddressFormat.Services
{
    public class RegionFormat
    {
        private List<Region> _Citys;
        private List<Region> _Countys;
        private List<Region> _Towns;
        private Region _Province;
        private Region _City;
        private Region _County;
        private Region _Town;
        private string _Address;
        private int _P;
        private int _Index;

        /// <summary>
        /// 格式化地址为：省、市、县和街道详细地址
        /// </summary>
        /// <param name="address">地址字符串</param>
        /// <returns>Address 结构化地址数据</returns>
        public Address Format(string address)
        {
            // 清洗地址字符串中的无用字符
            var str = Regex.Replace(address ?? "", @"\s|(null)|'|""|,|;|\[|\]|^-+", "");
            _Address = Regex.Replace(Regex.Replace(Regex.Replace(str, @"^\d+", ""), @"^\(.*?\)|^（.*?）", ""), @"^中国", "");
            if (_Address.Length < 5) return SetRegion();

            FindProvinc();
            return SetRegion();
        }

        /// <summary>
        /// 查找省级区划
        /// </summary>
        /// <returns>bool 是否找到省级区划</returns>
        private void FindProvinc()
        {
            var list = (from p in Provinces
                        let index = GetIndex(p.Alias, 5)
                        where index >= 0
                        orderby index
                        select new { region = p, index }).ToList();
            if (!list.Any())
            {
                FindProvinceReverse();
                return;
            }

            var prov = list.First();
            _Province = prov.region;
            _Citys = Citys.Where(c => c.ParentId == _Province.ID).ToList();

            SetIndex(prov.index, _Province.Name, _Province.Alias);
            FindCity();
        }

        /// <summary>
        /// 查询市级区划
        /// </summary>
        private void FindCity()
        {
            var list = (from c in _Citys
                        let alias = c.Alias.Split(',').FirstOrDefault(_Address.Contains)
                        let key = alias ?? c.Name
                        let index = GetIndex(key)
                        where index >= 0
                        orderby index
                        select new { region = c, index, key }).ToList();
            if (!list.Any())
            {
                FindCityReverse();
                return;
            }

            var city = list.First();
            _City = city.region;
            _Countys = Countys.Where(c => c.ParentId == _City.ID).ToList();

            SetIndex(city.index, _City.Name, city.key);
            FindCounty();
        }

        /// <summary>
        /// 查询县级区划
        /// </summary>
        private void FindCounty()
        {
            var list = (from c in _Countys
                        let alias = c.Alias.Split(',').FirstOrDefault(_Address.Contains)
                        let key = alias ?? c.Name
                        let index = GetIndex(key)
                        where index >= 0
                        orderby index
                        select new { region = c, index, key }).ToList();
            if (!list.Any())
            {
                FindCountyReverse();
                return;
            }

            var county = list.First();
            _County = county.region;
            _Towns = Towns.Where(t => t.ParentId == _County.ID).ToList();

            SetIndex(county.index, _County.Name, county.key);
            FindTown();
        }

        /// <summary>
        /// 查询镇级区划
        /// </summary>
        private void FindTown()
        {
            var list = (from t in _Towns
                        let alias = t.Alias.Split(',').FirstOrDefault(_Address.Contains)
                        let key = alias ?? t.Name
                        let index = GetIndex(key)
                        where index >= 0
                        orderby index
                        select new { region = t, index, key }).ToList();
            if (!list.Any()) return;

            var town = list.First();
            _Town = town.region;
            _Index = town.index;
        }

        /// <summary>
        /// 在省级区划范围内尝试通过县级区划反向查询市级区划
        /// </summary>
        private void FindCityReverse()
        {
            var list = (from r in _Citys
                        join c in Countys on r.ID equals c.ParentId
                        let alias = c.Alias.Split(',').FirstOrDefault(_Address.Contains)
                        let key = alias ?? c.Name
                        let index = GetIndex(key)
                        where index >= 0
                        orderby index
                        select new { region = c, index, key }).ToList();
            if (!list.Any())
            {
                if (!"北京,天津,上海,重庆".Contains(_Province.Alias)) return;

                _City = Citys.Single(c => c.ParentId == _Province.ID && c.Name == "市辖区");
                return;
            }

            var county = list.First();
            _County = county.region;
            _City = Citys.Single(c => c.ID == _County.ParentId);
            _Towns = Towns.Where(t => t.ParentId == _County.ID).ToList();

            SetIndex(county.index, _County.Name, county.key);
            FindTown();
        }

        /// <summary>
        /// 在市级区划范围内尝试通过镇级区划反向查询县级区划
        /// </summary>
        private void FindCountyReverse()
        {
            var list = (from c in _Countys
                        join t in Towns on c.ID equals t.ParentId
                        let alias = t.Alias.Split(',').FirstOrDefault(_Address.Contains)
                        let key = alias ?? t.Name
                        let index = GetIndex(key)
                        where index >= 0
                        orderby index
                        select new { region = t, index, key }).ToList();
            if (!list.Any()) return;

            var town = list.First();
            _Town = town.region;
            _Index = town.index;
            _County = Countys.Single(c => c.ID == town.region.ParentId);
        }

        /// <summary>
        /// 根据市级区划ID反向查询省级区划
        /// </summary>
        private void FindProvinceReverse()
        {
            var list = (from c in Citys
                        let alias = c.Alias.Split(',').FirstOrDefault(_Address.Contains)
                        let key = alias ?? c.Name
                        let index = GetIndex(key)
                        where index >= 0
                        orderby index
                        select new { region = c, index, key }).ToList();
            if (!list.Any())
            {
                ReverseFromCounty();
                return;
            }

            var city = list.First();
            _City = city.region;
            _Province = Provinces.Single(p => p.ID == _City.ParentId);
            _Countys = Countys.Where(c => c.ParentId == _City.ID).ToList();

            SetIndex(city.index, _City.Name, city.key);
            FindCounty();
        }

        /// <summary>
        /// 通过县级行政区划反查省市
        /// </summary>
        /// <returns>Address 结构化地址数据</returns>
        private void ReverseFromCounty()
        {
            var list = (from c in Countys
                        let alias = c.Alias.Split(',').FirstOrDefault(_Address.Contains)
                        let key = alias ?? c.Name
                        let index = GetIndex(key)
                        where index >= 0
                        orderby index
                        select new { region = c, index, key }).ToList();
            if (!list.Any()) return;

            var county = list.First();
            _County = county.region;
            _City = Citys.Single(c => c.ID == _County.ParentId);
            _Province = Provinces.Single(p => p.ID == _City.ParentId);

            SetIndex(county.index, _County.Name, county.key);
            FindTown();
        }

        /// <summary>
        /// 设置断句点
        /// </summary>
        /// <param name="index">匹配起始位置</param>
        /// <param name="name">区划名称</param>
        /// <param name="alias">区划别名</param>
        private void SetIndex(int index, string name, string alias)
        {
            var last = _Address.Length - index;
            var count = last < name.Length ? last : name.Length;
            var i = _Address.IndexOf(name, index, count, StringComparison.OrdinalIgnoreCase);
            _Index = index;
            _P = i >= 0 ? i + name.Length : index + alias.Length;
        }

        /// <summary>
        /// 获取关键词在字符串中的位置
        /// </summary>
        /// <param name="key">关键词</param>
        /// <param name="count">查询字符数</param>
        /// <returns>int 关键词位置</returns>
        private int GetIndex(string key, int count = 0)
        {
            var last = _Address.Length - _P;
            if (count == 0) count = key.Length + 9;

            count = last < count ? last : count;
            return _Address.IndexOf(key, _P, count, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 构造结构化地址数据并返回
        /// </summary>
        /// <returns>Address 结构化地址数据</returns>
        private Address SetRegion()
        {
            var region = new Address
            {
                Province = _Province?.Name,
                City = _City?.Name,
                County = _County?.Name,
                Town = _Town?.Name,
                Street = _Address.Substring(_Index)
            };
            return region;
        }
    }
}
