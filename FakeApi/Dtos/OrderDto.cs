using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FakeApi.Dtos
{
	public class OrderDto
	{
		public Guid Id { get; set; }

		public string UserId { get; set; }

		public ICollection<LineItemDto> OrderItems { get; set; }

		public string State { get; set; } // 使用有現狀態機

		public DateTime CreateDateUTC { get; set; }

		public string TransactionMetadata { get; set; } // 第三方支付
	}
}
