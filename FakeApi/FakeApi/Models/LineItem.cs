using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FakeApi.Models
{
	public class LineItem
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 整數組件說明
		public int Id { get; set; }

		[ForeignKey("TouristRouteId")]
		public Guid TouristRouteId { get; set; }

		public TouristRoute TouristRoute { get; set; }

		public Guid? ShoppingCartId { get; set; }

		//public Guid? OrderId { get; set; }

		[Column(TypeName = "decimal(18,2)")] // 兩位小數
		public decimal OriginalPrice { get; set; }

		[Range(0.0, 1.0)] // 0到1的小數
		public double? DiscountPresent { get; set; }
	}
}
