using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FakeApi.Models
{
	public class TouristRoutePicture
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 自動累加產生序號
		public int Id { get; set; }

		[MaxLength(100)]
		public string Url { get; set; }

		[ForeignKey("TouristRouteId")] // 外鍵聯繫 會主動將類名(TouristRoute)&變數名(Id)相連 所以 = TouristRouteId
		public Guid TouristRouteId { get; set; }

		public TouristRoute TourisRoutes { get; set; }
	}
}
