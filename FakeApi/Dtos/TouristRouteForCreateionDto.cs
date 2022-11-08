using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FakeApi.ValidationAttributes;

namespace FakeApi.Dtos
{
	//[TouristRouteTitleMustBeDifferentFromDescriptionAttribute]
	public class TouristRouteForCreateionDto : TouristRouteForMainpulationDto //: IValidatableObject 
	{
		//// 增加數據驗證規則 DataAnnotations(基本數據驗證) 字段
		//[Required(ErrorMessage = "title 不可為空")] // 不能為空
		//[MaxLength(100)] // 最大長度100
		//public string Title { get; set; }

		//[Required] // 不能為空
		//[MaxLength(1500)] // 最大長度1500
		//public string Description { get; set; }

		//// 計算方式:原價*折扣
		//public decimal Price { get; set; }

		//public DateTime CreateTime { get; set; }

		//public DateTime? UpdateTime { get; set; }

		//public DateTime? DepartureTime { get; set; }

		//public string Features { get; set; }

		//public string Fees { get; set; }

		//public string Notes { get; set; }

		//public double? Rating { get; set; }

		//public string TravelDays { get; set; }

		//public string TripType { get; set; }

		//public string DepartureCity { get; set; }

		//public ICollection<TouristRoutePictureForCreationDto> TouristRoutePictures { get; set; }
		//	= new List<TouristRoutePictureForCreationDto>();

		//// 數據驗證 (透過繼承的IValidatableObject function驗證數據)
		////public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		////{
		////	if(Title == Description)
		////	{
		////		yield return new ValidationResult( // yield 確保在錯誤之後繼續執行
		////			"路線名稱必須與路線描述不同", // 錯誤訊息
		////			new[] { "TouristRouteForCreateionDto" } // 錯誤路徑
		////		);
		////	}
		////}
	}
}
