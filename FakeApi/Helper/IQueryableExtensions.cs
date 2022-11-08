using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeApi.Services;
using System.Linq.Dynamic.Core;

namespace FakeApi.Helper
{
	
	public static class IQueryableExtensions
	{
		/// <summary>
		/// 用於數據排序
		/// </summary>
		public static IQueryable<T> ApplySort<T>(
			this IQueryable<T> source, // 代表擴展 IQueryable<T>
			string orderBy,
			Dictionary<string,PropertyMappingValue> mappingDictionary
		)
		{
			// 檢查傳入數據 是否有效
			if(source == null)
			{
				throw new ArgumentException("source");
			}

			// 檢查映射字串是否為空
			if(mappingDictionary == null )
			{
				throw new ArgumentException("mappingDictionary");
			}

			// 檢查排序字串是否為空
			if(string.IsNullOrWhiteSpace(orderBy))
			{
				return source;
			}

			var orderByString = string.Empty;

			var orderByAfterSplit = orderBy.Split(',');

			foreach(var order in orderByAfterSplit)
			{
				var trimmedOrder = order.Trim();

				// 通過字符串"desc" 來判斷升序還是降序
				var orderDescending = trimmedOrder.EndsWith("desc");

				// 刪除升序或降序字串 "asc" or "desc" 來獲得屬性的名稱
				var indexOfFirstSpace = trimmedOrder.IndexOf(" ");
				var propertyName = indexOfFirstSpace == -1 ? trimmedOrder : trimmedOrder.Remove(indexOfFirstSpace);
				if(!mappingDictionary.ContainsKey(propertyName))
				{
					throw new ArgumentException($"Key mapping for {propertyName} is missing");
				}

				var propertyMappingValue = mappingDictionary[propertyName];
				if(propertyMappingValue == null)
				{
					throw new ArgumentException("propertyMappingValue");
				}

				foreach(var destinationProperty in 
					propertyMappingValue.DestinationProperties.Reverse())
				{
					// 給IQueryable 添加排序字符串
					orderByString = orderByString +
						(string.IsNullOrWhiteSpace(orderByString) ? string.Empty : ", ")
						+ destinationProperty
						+ (orderDescending ? " descending" : " ascending");
				}	
			}
			return source.OrderBy(orderByString);
		}
	}
}
