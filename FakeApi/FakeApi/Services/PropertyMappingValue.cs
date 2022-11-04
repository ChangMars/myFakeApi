using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FakeApi.Services
{
	public class PropertyMappingValue
	{
		public IEnumerable<string> DestinationProperties { get; private set; } // 排序的目標屬性
		public PropertyMappingValue(IEnumerable<string> destinationProperties)
		{
			DestinationProperties = destinationProperties;
		}
	}
}
