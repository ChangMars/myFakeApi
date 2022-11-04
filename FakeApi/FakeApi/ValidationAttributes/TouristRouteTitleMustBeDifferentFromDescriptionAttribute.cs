using System.ComponentModel.DataAnnotations;
using FakeApi.Dtos;

namespace FakeApi.ValidationAttributes
{
	/// <summary>
	/// 新增類級別的數據驗證
	/// </summary>
	public class TouristRouteTitleMustBeDifferentFromDescriptionAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(
			object value, 
			ValidationContext validationContext)
		{
			//return base.IsValid(value, validationContext);
			var touristRouteDto = (TouristRouteForMainpulationDto)validationContext.ObjectInstance;
			if (touristRouteDto.Title == touristRouteDto.Description)
			{
				return new ValidationResult( // yield 確保在錯誤之後繼續執行
					"路線名稱必須與路線描述不同", // 錯誤訊息
					new[] { "TouristRouteForCreateionDto" } // 錯誤路徑
				);
			}
			return ValidationResult.Success;
		}
	}
}
