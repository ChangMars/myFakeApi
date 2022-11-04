using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FakeApi.Helper
{
	public static class IEnumerableExtensions
	{
		/// <summary>
		/// 用於數據塑形
		/// </summary>
		public static IEnumerable<ExpandoObject> ShapeData<TSource>(
			this IEnumerable<TSource> source,
			string fields)
		{
			// 檢查傳入數據 是否有效
			if (source == null)
			{
				throw new ArgumentException("source");
			}

			// 創建列表類型本地變量來保存動態類型對象
			var expandoObjectList = new List<ExpandoObject>();

			// 避免在列表遍歷數據，創建一個屬性列表
			var propertyInfoList = new List<PropertyInfo>();

			if(string.IsNullOrWhiteSpace(fields)) //限定欄位為空(不限定輸出欄位的意思)
			{
				// 希望返回動態類型對象ExpandoObject 所有屬性
				var propertyInfos = typeof(TSource)
					.GetProperties(BindingFlags.IgnoreCase // 忽綠大小寫
					| BindingFlags.Public // 獲取公有成員
					| BindingFlags.Instance // 獲取實例化對象
					);
				propertyInfoList.AddRange(propertyInfos); // 加入屬性列表中
			}
			else// 限定輸出欄位
			{
				// 逗號來分割字段字符串
				var fieldsAfterSplit = fields.Split(',');

				foreach(var field in fieldsAfterSplit)
				{
					// 去除首尾多餘的空格，獲得欄位名稱
					var propertyName = field.Trim();

					var propertyInfo = typeof(TSource)
						.GetProperty(propertyName, BindingFlags.IgnoreCase // 忽綠大小寫
					| BindingFlags.Public // 獲取公有成員
					| BindingFlags.Instance // 獲取實例化對象
					);

					if(propertyInfo == null)
					{
						throw new Exception($"屬性 {propertyName} 找不到 {typeof(TSource)}");
					}

					propertyInfoList.Add(propertyInfo); // 加入屬性列表中
				}
			}

			foreach(TSource sourceObject in source)
			{
				// 創建動態類型對象，創建數據塑形對象
				var dataShapedObject = new ExpandoObject();
				foreach (var propertyInfo in propertyInfoList)
				{
					// 獲取對應屬性的真實數據
					var propertyValue = propertyInfo.GetValue(sourceObject);

					((IDictionary<string, object>)dataShapedObject)
						.Add(propertyInfo.Name, propertyValue);
				}
				expandoObjectList.Add(dataShapedObject);
			}
			return expandoObjectList;
		}
	}
}
