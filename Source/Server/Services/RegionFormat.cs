using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Insight.Utils.AddressFormat.Entity;
using Insight.WS.Utils.Entity;

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
            var str = Regex.Replace(address ?? "", @"\s|(null)|(&nbsp;)|'|""|,|;|\[|\]|^-+", "");
            _Address = Regex.Replace(Regex.Replace(Regex.Replace(str, @"^\d+", ""), @"^\(.*?\)|^（.*?）", ""), @"^中国", "");
            if (_Address.Length < 5) return SetRegion();

            // 尝试匹配省级区划
            FindProvinc();

            // 根据省级区划正查市级区划或尝试通过市级区划反查省级区划
            if (_Province != null) FindCity();
            else FindProvinceReverse();

            // 未匹配到省级区划，尝试从县级区划反查省市并返回结构化地址数据
            if (_Province == null) return ReverseFromCounty();

            // 根据市级区划正查县级区划或尝试通过县级区划反查市级区划
            if (_City != null) FindCounty();
            else FindCityReverse();

            // 如未匹配到市级区划，尝试从镇级区划反查县市并返回结构化地址数据
            if (_City == null) return ReverseFromTown();

            // 根据县级区划正查级镇区划或尝试通过镇级区划反查县级区划
            if (_County == null) FindCountyReverse();
            else FindTown();

            return SetRegion();
        }

        /// <summary>
        /// 查找省级区划
        /// </summary>
        /// <returns>bool 是否找到省级区划</returns>
        private void FindProvinc()
        {
            var list = (from p in Parms.Provinces
                let index = GetIndex(p.Alias, 5)
                where index >= 0
                orderby index
                select new {region = p, index}).ToList();
            if (!list.Any()) return;

            var prov = list.First();
            _Province = prov.region;
            _Citys = Parms.Citys.Where(c => c.ParentId == _Province.ID).ToList();
            SetIndex(prov.index, _Province.Name, _Province.Alias);
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
                select new {region = c, index, key}).ToList();
            if (!list.Any()) return;

            var city = list.First();
            _City = city.region;
            _Countys = Parms.Countys.Where(c => c.ParentId == _City.ID).ToList();
            SetIndex(city.index, _City.Name, city.key);
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
                select new {region = c, index, key}).ToList();
            if (!list.Any()) return;

            var county = list.First();
            _County = county.region;
            _Towns = Parms.Towns.Where(t => t.ParentId == _County.ID).ToList();
            SetIndex(county.index, _County.Name, county.key);
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
                select new {region = t, index, key}).ToList();
            if (!list.Any()) return;

            var town = list.First();
            _Town = town.region;
            _Index = town.index;
        }

        /// <summary>
        /// 根据市级区划ID反向查询省级区划
        /// </summary>
        private void FindProvinceReverse()
        {
            var list = (from c in Parms.Citys
                let alias = c.Alias.Split(',').FirstOrDefault(_Address.Contains)
                let key = alias ?? c.Name
                let index = GetIndex(key)
                where index >= 0
                orderby index
                select new {region = c, index, key}).ToList();
            if (!list.Any()) return;

            var city = list.First();
            _City = city.region;
            _Countys = Parms.Countys.Where(c => c.ParentId == _City.ID).ToList();
            _Province = Parms.Provinces.Single(p => p.ID == _City.ParentId);
            _Citys = Parms.Citys.Where(c => c.ParentId == _Province.ID).ToList();
            SetIndex(city.index, _City.Name, city.key);
        }

        /// <summary>
        /// 在省级区划范围内尝试通过县级区划反向查询市级区划
        /// </summary>
        private void FindCityReverse()
        {
            var list = (from r in _Citys
                join c in Parms.Countys on r.ID equals c.ParentId
                let alias = c.Alias.Split(',').FirstOrDefault(_Address.Contains)
                let key = alias ?? c.Name
                let index = GetIndex(key)
                where index >= 0
                orderby index
                select new {region = c, index, key}).ToList();
            if (!list.Any()) return;

            var county = list.First();
            _County = county.region;
            _Towns = Parms.Towns.Where(t => t.ParentId == _County.ID).ToList();
            _City = Parms.Citys.Single(c => c.ID == _County.ParentId);
            _Countys = Parms.Countys.Where(c => c.ParentId == _City.ID).ToList();
            SetIndex(county.index, _County.Name, county.key);
        }

        /// <summary>
        /// 在市级区划范围内尝试通过镇级区划反向查询县级区划
        /// </summary>
        private void FindCountyReverse()
        {
            var list = (from c in _Countys
                join t in Parms.Towns on c.ID equals t.ParentId
                let alias = t.Alias.Split(',').FirstOrDefault(_Address.Contains)
                let key = alias ?? t.Name
                let index = GetIndex(key)
                where index >= 0
                orderby index
                select new {region = t, index, key}).ToList();
            if (!list.Any()) return;

            var town = list.First();
            _Town = town.region;
            _Index = town.index;
            _County = Parms.Countys.Single(c => c.ID == town.region.ParentId);
        }

        /// <summary>
        /// 通过县级行政区划反查省市
        /// </summary>
        /// <returns>Address 结构化地址数据</returns>
        private Address ReverseFromCounty()
        {
            var list = (from c in Parms.Countys
                let alias = c.Alias.Split(',').FirstOrDefault(_Address.Contains)
                let key = alias ?? c.Name
                let index = GetIndex(key)
                where index >= 0
                orderby index
                select new {region = c, index, key}).ToList();
            if (!list.Any()) return SetRegion();

            var keys = list.GroupBy(r => r.key);
            if (list.Count == 1 || keys.Count() > 1)
            {
                var county = list.First();
                _County = county.region;
                _Towns = Parms.Towns.Where(t => t.ParentId == _County.ID).ToList();
                SetIndex(county.index, _County.Name, county.key);
                FindTown();
            }
            else
            {
                var towns = (from c in list
                    join t in Parms.Towns on c.region.ID equals t.ParentId
                    let alias = t.Alias.Split(',').FirstOrDefault(_Address.Contains)
                    let key = alias ?? t.Name
                    let index = GetIndex(key)
                    where index >= 0
                    orderby index
                    select new {region = t, index, key}).ToList();
                if (!towns.Any()) return SetRegion();

                keys = towns.GroupBy(r => r.key);
                if (towns.Count > 1 && keys.Count() == 1) return SetRegion();

                var town = towns.First();
                _Town = town.region;
                _Index = town.index;
                _County = Parms.Countys.Single(c => c.ID == town.region.ParentId);
            }

            _City = Parms.Citys.Single(c => c.ID == _County.ParentId);
            _Province = Parms.Provinces.Single(p => p.ID == _City.ParentId);
            return SetRegion();
        }

        /// <summary>
        /// 通过镇级行政区划反查县市。如未能查到且省级区划为直辖市，则市级区划设置为市辖区
        /// </summary>
        /// <returns></returns>
        private Address ReverseFromTown()
        {
            var list = (from c in _Citys
                join x in Parms.Countys on c.ID equals x.ParentId
                join t in Parms.Towns on x.ID equals t.ParentId
                let alias = t.Alias.Split(',').FirstOrDefault(_Address.Contains)
                let key = alias ?? t.Name
                let index = GetIndex(key)
                where index >= 0
                orderby index
                select new {region = t, index, key}).ToList();
            if (list.Any())
            {
                var keys = list.GroupBy(r => r.key);
                if (list.Count == 1 || keys.Count() > 1)
                {
                    var town = list.First();
                    _Town = town.region;
                    _Index = town.index;
                    _County = Parms.Countys.Single(c => c.ID == town.region.ParentId);
                    _City = Parms.Citys.Single(c => c.ID == _County.ParentId);
                    return SetRegion();
                }
            }

            if ("北京,天津,上海,重庆".Contains(_Province.Alias))
            {
                _City = Parms.Citys.Single(c => c.ParentId == _Province.ID && c.Name == "市辖区");
            }
            return SetRegion();
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
        /// 获取关键词在字符串中的c
        /// </summary>
        /// <param name="key">关键词</param>
        /// <param name="count">查询字符数</param>
        /// <returns>int 关键词</returns>
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
