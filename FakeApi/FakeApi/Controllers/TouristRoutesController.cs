using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using FakeApi.Dtos;
using FakeApi.Helper;
using FakeApi.Models;
using FakeApi.ResourceParameters;
using FakeApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Net.Http.Headers;

namespace FakeApi.Controllers
{
    [Route("api/[controller]")] // api/touristroutes
    [ApiController]
    public class TouristRoutesController : ControllerBase
    {
		#region 注入屬性映射服務
		private ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;
		private readonly IUrlHelper _urlHelper;
		private readonly IPropertyMappingService _propertyMappingService;

        public TouristRoutesController(ITouristRouteRepository touristRouteRepository
			,IMapper mapper
			,IUrlHelperFactory urlHelperFactory
			,IActionContextAccessor actionContextAccessor
			,IPropertyMappingService propertyMappingService)
        {
            _touristRouteRepository = touristRouteRepository;
            _mapper = mapper;
			_urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
			_propertyMappingService = propertyMappingService;
        }
		#endregion

		/// <summary>
		/// 分頁導航
		/// </summary>
		/// <param name="paramaters"></param>
		/// <param name="paramaters2"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private string GenerateTouristRouteResourceURL(
			TouristRouteResourceParamaters paramaters,
			PaginationResourceParamaters paramaters2,
			ResourceUrlType type
		)
		{
			// 使用link生成絕對路徑url
			return type switch
			{
				ResourceUrlType.PreviousPage => _urlHelper.Link("GetTouristRoutes",
					new
					{
						files = paramaters.Fields,
						orderBy = paramaters.OrderBy,
						keyword = paramaters.Keyword,
						rating = paramaters.Rating,
						pageNumber = paramaters2.PageNumber - 1,
						pageSize = paramaters2.PageSize
					}),
				ResourceUrlType.NextPage => _urlHelper.Link("GetTouristRoutes",
					new
					{
						files = paramaters.Fields,
						orderBy = paramaters.OrderBy,
						keyword = paramaters.Keyword,
						rating = paramaters.Rating,
						pageNumber = paramaters2.PageNumber + 1,
						pageSize = paramaters2.PageSize
					}),
				_ => _urlHelper.Link("GetTouristRoutes",
					new
					{
						files = paramaters.Fields,
						orderBy = paramaters.OrderBy,
						keyword = paramaters.Keyword,
						rating = paramaters.Rating,
						pageNumber = paramaters2.PageNumber,
						pageSize = paramaters2.PageSize
					})
			};

		}

		// api/touristRoutes?keyword=傳入參數
		// application/json -> 旅遊路線資源
		// application/vnd.{公司名稱}.hateoas+json -> 輸出包含超連結資料
		// application/vnd.{公司名稱}.touristRoute.simplify+json -> 輸出簡化版資源數據
		// application/vnd.{公司名稱}.touristRoute.simplify.hateoas+json ->輸出簡化版包含超連結資料
		[Produces( // 註冊自定義媒體類型(格式處理器)(個別添加)
			"application/json",
			"application/vnd.rong.hateoas+json",
			"application/vnd.rong.touristRoute.simplify+json",
			"application/vnd.rong.touristRoute.simplify.hateoas+json"
			)]
		[HttpGet(Name = "GetTouristRoutes")]
		[HttpHead]
		public async Task<IActionResult> GetTouristRoutes(
			[FromQuery] TouristRouteResourceParamaters paramaters, // 小於 lessThan,大於 largerThan,等於 equal To lessThan3
			[FromQuery] PaginationResourceParamaters paramaters2,
			[FromHeader(Name = "Accept")] string mediaType) // 獲取媒體類型
		{
			// 檢查header 是否帶有媒體類型
			if(!MediaTypeHeaderValue
				.TryParse(mediaType,out MediaTypeHeaderValue parsedMediatype))
			{
				return BadRequest();
			}
			// 檢查輸入的排序字串是否正確
			if(!_propertyMappingService.IsMappingExist<TouristRouteDto,TouristRoute>(
				paramaters.OrderBy))
			{
				return BadRequest("請輸入正確的排序參數");
			}

			if (!_propertyMappingService.IsPropertiesExists<TouristRouteDto>(
				paramaters.Fields))
			{
				return BadRequest("請輸入正確的塑型參數");
			}

			var touristRoutesFromRepo = await _touristRouteRepository
				.GetTouristRoutesAsync(
					paramaters.Keyword, 
					paramaters.RatingOperator, 
					paramaters.RatingValue,
					paramaters2.PageSize,
					paramaters2.PageNumber,
					paramaters.OrderBy
				);
            if(touristRoutesFromRepo == null || touristRoutesFromRepo.Count() <= 0)
			{
                return NotFound("沒有旅遊路線");
			}

			var previousPageLink = touristRoutesFromRepo.HasPrevious
				? GenerateTouristRouteResourceURL(
					paramaters, paramaters2, ResourceUrlType.PreviousPage)
					: null;
			var nextPageLink = touristRoutesFromRepo.HasNext
				? GenerateTouristRouteResourceURL(
					paramaters, paramaters2, ResourceUrlType.NextPage)
					: null;

			// x-pagination
			var paginationMetadata = new
			{
				previousPageLink,
				nextPageLink,
				totalCount = touristRoutesFromRepo.TotalCount,
				pageSize = touristRoutesFromRepo.PageSize,
				currentPage = touristRoutesFromRepo.CurrentPage,
				totalPages = touristRoutesFromRepo.TotalPages
			};

			Response.Headers.Add("x-pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

			//return Ok(touristRoutesDto.ShapeData(paramaters.Fields));

			//判斷是否為 hateoas 媒體類型
			bool isHateoas = parsedMediatype.SubTypeWithoutSuffix
				.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

			// 獲取主要的媒體類型
			var primaryMediaType = isHateoas ?
				parsedMediatype.SubTypeWithoutSuffix.Substring(0, parsedMediatype.SubTypeWithoutSuffix.Length - 8)
				: parsedMediatype.SubTypeWithoutSuffix;

			//var touristRoutesDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);
			//var shapedDtoList = touristRoutesDto.ShapeData(paramaters.Fields);

			// 使用泛型來變數來存放資料
			IEnumerable<object> touristRoutesDto;
			IEnumerable<ExpandoObject> shapedDtoList;

			if(primaryMediaType == "vnd.rong.touristRoute.simplify")
			{
				touristRoutesDto = _mapper.Map<IEnumerable<TouristRouteSimplifyDto>>(touristRoutesFromRepo);
				shapedDtoList = ((IEnumerable<TouristRouteSimplifyDto>)touristRoutesDto).ShapeData(paramaters.Fields);
			}
			else
			{
				touristRoutesDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);
				shapedDtoList = ((IEnumerable<TouristRouteDto>)touristRoutesDto).ShapeData(paramaters.Fields);
			}

			//if (parsedMediatype.MediaType == "application/vnd.rong.hateoas+json")
			if(isHateoas)
			{
				var linkDto = CreateLinksForTouristRouteList(paramaters, paramaters2);

				var shapeDtoWithLinklist = shapedDtoList.Select(t =>
				{
					var touristRouteDictionary = t as IDictionary<string, object>;
					var links = CreateLinkForTouristRoute((Guid)touristRouteDictionary["Id"], null);
					touristRouteDictionary.Add("links", links);
					return touristRouteDictionary;
				});

				var result = new
				{
					value = shapeDtoWithLinklist,
					links = linkDto
				};

				return Ok(result);
			}

			return Ok(shapedDtoList);
        }

		/// <summary>不使用AutoMapper的方式</summary>
		//[HttpGet("{touristRouteId}")]
		//public IActionResult GetTouristRouteById(Guid touristRouteId)
		//{
		//    var touristRouteFromRepo = _touristRouteRepository.GetTouristRoute(touristRouteId);
		//    if (touristRouteFromRepo == null)
		//    {
		//        return NotFound($"旅游路线{touristRouteId}找不到");
		//    }

		//    var touristRouteDto = new TouristRouteDto()
		//    {
		//        Id = touristRouteFromRepo.Id,
		//        Title = touristRouteFromRepo.Title,
		//        Description = touristRouteFromRepo.Description,
		//        Price = touristRouteFromRepo.OriginalPrice * (decimal)(touristRouteFromRepo.DiscountPresent ?? 1),
		//        CreateTime = touristRouteFromRepo.CreateTime,
		//        UpdateTime = touristRouteFromRepo.UpdateTime,
		//        Features = touristRouteFromRepo.Features,
		//        Fees = touristRouteFromRepo.Fees,
		//        Notes = touristRouteFromRepo.Notes,
		//        Rating = touristRouteFromRepo.Rating,
		//        TravelDays = touristRouteFromRepo.TravelDays.ToString(),
		//        TripType = touristRouteFromRepo.TripType.ToString(),
		//        DepartureCity = touristRouteFromRepo.DepartureCity.ToString()
		//    };

		//    return Ok(touristRouteDto);
		//}


		/// <summary>使用AutoMapper的方式</summary>
		[HttpGet("{touristRouteId}",Name = "GetTouristRouteById")] // api/touristroutes/{touristRouteId}
		//[HttpHead]
		public async Task<IActionResult> GetTouristRouteById(
			Guid touristRouteId,
			string fields)
		{
			var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
			if (touristRouteFromRepo == null)
			{
				return NotFound($"旅遊路線{touristRouteId}找不到");
			}
			var touristRouteDto = _mapper.Map<TouristRouteDto>(touristRouteFromRepo);
			//return Ok(touristRouteDto.ShapeData(fields));

			var linkDtos = CreateLinkForTouristRoute(touristRouteId, fields);

			var result = touristRouteDto.ShapeData(fields)
				as IDictionary<string,object>;
			result.Add("links", linkDtos);
			return Ok(result);
		}

		/// <summary>
		/// 創建單獨資源的link 資源
		/// </summary>
		/// <param name="touristRouteId"></param>
		/// <param name="fields"></param>
		/// <returns></returns>
		private IEnumerable<LinkDto> CreateLinkForTouristRoute(
			Guid touristRouteId,
			string fields)
		{
			var links = new List<LinkDto>();

			links.Add(
				new LinkDto(
					Url.Link("GetTouristRouteById", new { touristRouteId, fields }),
					"self",
					"GET"
					)
				);

			// 更新
			links.Add(
				new LinkDto(
					Url.Link("UpdateTouristRoute", new { touristRouteId }),
					"update",
					"PUT"
					)
				);

			// 局部更新 
			links.Add(
				new LinkDto(
					Url.Link("PartiallyUpdateTouristRoute", new { touristRouteId }),
					"partially_update",
					"PATCH")
				);

			// 删除
			links.Add(
				new LinkDto(
					Url.Link("DeleteTouristRoute", new { touristRouteId }),
					"delete",
					"DELETE")
				);

			// 获取路线图片
			links.Add(
				new LinkDto(
					Url.Link("GetPictureListForTouristRoute", new { touristRouteId }),
					"get_pictures",
					"GET")
				);

			// 添加新图片
			links.Add(
				new LinkDto(
					Url.Link("CreateTouristRoutePicture", new { touristRouteId }),
					"create_picture",
					"POST")
				);

			return links;
		}

		private IEnumerable<LinkDto> CreateLinksForTouristRouteList(
			TouristRouteResourceParamaters paramaters,
			PaginationResourceParamaters paramaters2)
		{
			var links = new List<LinkDto>();
			// 添加自我鏈結
			links.Add(new LinkDto(
				GenerateTouristRouteResourceURL(paramaters, paramaters2, ResourceUrlType.CurrentPage),
				"self",
				"GET"
			));

			// "api/touristRoutes"
			// 添加創建旅遊路線
			links.Add(new LinkDto(
				Url.Link("CreateTouristRoute",null),
				"create_tourist_route",
				"POST"
			));
			return links;
		}

		[HttpPost(Name = "CreateTouristRoute")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> CreateTouristRoute([FromBody] TouristRouteForCreateionDto touristRouteForCreateionDto)
		{
			var touristRouteModel = _mapper.Map<TouristRoute>(touristRouteForCreateionDto);
			_touristRouteRepository.AddTouristRoute(touristRouteModel); // 添加數據至DbContext中
			await _touristRouteRepository.SaveAsync(); // 數據寫入Db
			var touristRouteToReture = _mapper.Map<TouristRouteDto>(touristRouteModel);

			//return CreatedAtRoute("GetTouristRouteById", 
			//	new { touristRouteId = touristRouteToReture.Id },
			//	touristRouteToReture);

			var links = CreateLinkForTouristRoute(touristRouteModel.Id, null); // 創建hatoeas link

			var result = touristRouteToReture.ShapeData(null) 
				as IDictionary<string,object>;

			result.Add("links", links); // 將返回數據加入 hatoeas link

			return CreatedAtRoute("GetTouristRouteById",
				new { touristRouteId = result["Id"] },
				result);
		}

		[HttpPut("{touristRouteId}", Name = "UpdateTouristRoute")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateTouristRoute(
			[FromRoute] Guid touristRouteId, 
			[FromBody] TouristRouteForUpdateDto touristRouteUpdateDto)
		{
			if (!await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))
			{
				return NotFound($"旅遊路線{touristRouteId}找不到");
			}

			var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
			// 1. 所以數據提取 映射dto
			// 2. 更新dto數據
			// 3. 映射model
			_mapper.Map(touristRouteUpdateDto, touristRouteFromRepo);

			await _touristRouteRepository.SaveAsync(); // 更新數據

			return NoContent();
		}

		[HttpPatch("{touristRouteId}", Name = "PartiallyUpdateTouristRoute")]
		public async Task<IActionResult> PartiallyUpdateTouristRoute(
			[FromRoute] Guid touristRouteId,
			[FromBody] JsonPatchDocument<TouristRouteForUpdateDto> patchDocument) // 這個要處理JsonPatch的數據驗證
		{
			if (!await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))
			{
				return NotFound($"旅遊路線{touristRouteId}找不到");
			}

			var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId); // 獲取數據
			var touristRouteToPatch = _mapper.Map<TouristRouteForUpdateDto>(touristRouteFromRepo); // 映射數據
			patchDocument.ApplyTo(touristRouteToPatch, ModelState); // 修改數據 
			if (!TryValidateModel(touristRouteToPatch)) // 用TryValidateModel來驗證修改後的數據
			{
				// TryValidateModel 會調用 ModelState進行數據驗證
				// 而ModelState是透過 ApplyTo 綁定時進行驗證
				// 而驗證規則為Dto DataAnnotations 定義的
				return ValidationProblem(ModelState);
			}
			_mapper.Map(touristRouteToPatch, touristRouteFromRepo); // 映射修改後的數據到先前數據
			await _touristRouteRepository.SaveAsync();

			return NoContent();
		}

		[HttpDelete("{touristRouteId}", Name = "DeleteTouristRoute")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteTouristRoute(
			[FromRoute] Guid touristRouteId)
		{
			if (!await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))
			{
				return NotFound($"旅遊路線{touristRouteId}找不到");
			}
			var touristRoute = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
			_touristRouteRepository.DeleteTouristRoute(touristRoute);
			await _touristRouteRepository.SaveAsync();

			return NoContent();
		}

		[HttpDelete("({touristIDs})")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteByIDs(
			[ModelBinder(BinderType = typeof(ArrayModelBinder))][FromRoute] IEnumerable<Guid> touristIDs)
		{
			if(touristIDs == null)
			{
				return BadRequest();
			}

			var touristRoutesFromRepo = await _touristRouteRepository.GetTouristRoutesByIDListAsync(touristIDs);
			_touristRouteRepository.DeleteTouristRoutes(touristRoutesFromRepo);
			await _touristRouteRepository.SaveAsync();

			return NoContent();

		}
	}
}
