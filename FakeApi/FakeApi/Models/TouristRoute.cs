using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FakeApi.Models
{
	public class TouristRoute
	{
		[Key]
		public Guid Id { get; set; }

		[Required] // 不能為空
		[MaxLength(100)] // 最大長度100
		public string Title { get; set; }

		[Required] // 不能為空
		[MaxLength(1500)] // 最大長度1500
		public string Description { get; set; }

		[Column(TypeName = "decimal(18,2)")] // 兩位小數
		public decimal OriginalPrice { get; set; }

		[Range(0.0, 1.0)] // 0到1的小數
		public double? DiscountPresent { get; set; }

		public DateTime CreateTime { get; set; }

		public DateTime? UpdateTime { get; set; }

		public DateTime? DepartureTime { get; set; }

		[MaxLength]
		public string Features { get; set; }

		[MaxLength]
		public string Fees { get; set; }

		[MaxLength]
		public string Notes { get; set; }

		public ICollection<TouristRoutePicture> TouristRoutePictures { get; set; }
			= new List<TouristRoutePicture>();

		public double? Rating { get; set; }

		public TravelDays? TravelDays { get; set; }

		public TripType? TripType { get; set; }

		public DepartureCity? DepartureCity { get; set; }

	}
}
