using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FakeApi.Services
{
	public class PropertyMapping<TSource, TDestination> : IPropertyMapping //因為C# 無法宣告 使用<TSouce,TDestination> 當作類型 所以使用一個接口來取代
	{
		public	Dictionary<string,PropertyMappingValue> _mappingDictionary { get; set; }
		public PropertyMapping(Dictionary<string,PropertyMappingValue> mappingDictionary)
		{
			_mappingDictionary = mappingDictionary;
		}
	}
}
