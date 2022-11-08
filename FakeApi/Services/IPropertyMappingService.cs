using System.Collections.Generic;

namespace FakeApi.Services
{
	public interface IPropertyMappingService
	{
		// 所有服務都是面向接口 所以都需添加 宣告
		Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>();
		bool IsMappingExist<TSource, TDestination>(string fields);
		bool IsPropertiesExists<T>(string fields);
	}
}