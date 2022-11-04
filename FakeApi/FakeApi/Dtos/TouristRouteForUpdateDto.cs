using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FakeApi.ValidationAttributes;

namespace FakeApi.Dtos
{
	public class TouristRouteForUpdateDto : TouristRouteForMainpulationDto
	{
		[Required(ErrorMessage = "更新必備")] // 不能為空
		[MaxLength(1500)] // 最大長度1500
		public override string Description { get; set; }
	}
}
