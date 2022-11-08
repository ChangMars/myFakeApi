using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FakeApi.Dtos;
using FakeApi.ResourceParameters;
using FakeApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FakeApi.Controllers
{
	[ApiController]
	[Route("api/orders")]
	public class OrdersController : ControllerBase
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ITouristRouteRepository _touristRouteRepository;
		private readonly IMapper _mapper;
		private readonly IHttpClientFactory _httpContextFactory;
		public OrdersController(IHttpContextAccessor httpContextAccessor,
			ITouristRouteRepository touristRouteRepository,
			IMapper mapper,
			IHttpClientFactory httpContextFactory)
		{
			_httpContextAccessor = httpContextAccessor;
			_touristRouteRepository = touristRouteRepository;
			_mapper = mapper;
			_httpContextFactory = httpContextFactory;
		}

		[HttpGet(Name = "GetOrders")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<IActionResult> GetOrders([FromQuery] PaginationResourceParamaters paramaters)
		{
			// 1. 獲取當前用戶
			var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

			// 2. 使用用戶ID來獲取訂單歷史紀錄
			var orders = await _touristRouteRepository.GetOrderByUserId(userId,paramaters.PageSize,paramaters.PageNumber);

			return Ok(_mapper.Map<IEnumerable<OrderDto>>(orders));
		}

		[HttpGet("{orderId}")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<IActionResult> GetOrderById([FromRoute] Guid orderId)
		{
			// 1. 獲取當前用戶
			var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

			// 2. 使用用戶ID來獲取訂單歷史紀錄
			var order = await _touristRouteRepository.GetOrderById(orderId);

			return Ok(_mapper.Map<OrderDto>(order));
		}

		[HttpPost("{orderId}/placeOrder")]
		[Authorize(AuthenticationSchemes = "Bearer")]
		public async Task<IActionResult> PlaceOrder([FromRoute] Guid orderId)
		{
			// 1. 獲得當前用戶
			var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

			// 2. 開始處理支付
			var order = await _touristRouteRepository.GetOrderById(orderId);
			order.PaymentProcessing();
			await _touristRouteRepository.SaveAsync();

			// 3. 向第三方提交支付請求,等待第三方響應
			var httpClient = _httpContextFactory.CreateClient();
			string url = @"https://localhost:5001/api/FakeVanderPaymentProcess?orderNumber={0}&returnFault={1}";
			var respone = await httpClient.PostAsync(
					string.Format(url, order.Id, false),
					null
				);

			// 4. 提取支付結果，以及支付信息
			bool isApproved = false;
			string transactionMetadata = "";
			if (respone.IsSuccessStatusCode)
			{
				transactionMetadata = await respone.Content.ReadAsStringAsync();
				var jsonObject = (JObject)JsonConvert.DeserializeObject(transactionMetadata);
				isApproved = jsonObject["approved"].Value<bool>();
			}

			// 5. 如果第三方支付成功，訂單完成
			if (isApproved)
			{
				order.PaymentApprove();
			}
			else
			{
				order.PaymentReject();
			}
			order.TransactionMetadata = transactionMetadata;
			await _touristRouteRepository.SaveAsync();

			return Ok(_mapper.Map<OrderDto>(order));
		}
	}
}
