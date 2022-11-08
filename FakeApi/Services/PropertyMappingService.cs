using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeApi.Dtos;
using FakeApi.Models;

namespace FakeApi.Services
{
	public class PropertyMappingService : IPropertyMappingService
	{
		// 定義資料表可以排序的項目
		private Dictionary<string, PropertyMappingValue> _touristRoutePropertyMapping =
			new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
			{
				{ "Id", new PropertyMappingValue(new List<string>() { "id" }) },
				{ "Title", new PropertyMappingValue(new List<string>() { "Title" }) },
				{ "Rating", new PropertyMappingValue(new List<string>() { "Rating" }) },
				{ "OriginalPrice", new PropertyMappingValue(new List<string>() { "OriginalPrice" }) },
			};

		private IList<IPropertyMapping> _propertyMappings = new List<IPropertyMapping>();

		public PropertyMappingService()
		{
			// 新增 屬性與數據關係
			_propertyMappings.Add(
				new PropertyMapping<TouristRouteDto, TouristRoute>(
					_touristRoutePropertyMapping));
		}

		public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
		{
			// 獲取匹配的映射對象
			var matchingMapping = _propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

			if (matchingMapping.Count() == 1)
			{
				return matchingMapping.First()._mappingDictionary;
			}
			throw new Exception(
				$"Cannot find exact property mapping instance for <{typeof(TSource)},{typeof(TDestination)}>");
		}

		public bool IsMappingExist<TSource,TDestination>(string fields)
		{
			var propertyMapping = GetPropertyMapping<TSource, TDestination>();

			if(string.IsNullOrWhiteSpace(fields))
			{ return true; }
			
			// 逗號來分割字段字符串
			var fieldsAfterSplit = fields.Split(",");
			
			// 檢查字符串是否含有定義欄位
			foreach (var field in fieldsAfterSplit)
			{
				// 去除空格
				var trimmedField = field.Trim();
				// 獲取字符串名稱字符串
				var indexOfFirstSpace = trimmedField.IndexOf(" ");
				var propertyName = indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace);

				if(! propertyMapping.ContainsKey(propertyName))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsPropertiesExists<T>(string fields)
		{
			if (string.IsNullOrWhiteSpace(fields))
			{ return true; }

			// 逗號來分割字段字符串
			var fieldsAfterSplit = fields.Split(",");

			// 檢查字符串是否含有定義欄位
			foreach (var field in fieldsAfterSplit)
			{
				// 去除空格
				var propertyName = field.Trim();

				var propertyInfo = typeof(T)
					.GetProperty(propertyName,
					BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

				// 如果T中沒找到對應的屬性
				if(propertyInfo == null)
				{
					return false;
				}	
			}
			return true;
		}
	}
}
