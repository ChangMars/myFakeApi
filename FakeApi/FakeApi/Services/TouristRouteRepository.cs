using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeApi.Database;
using FakeApi.Dtos;
using FakeApi.Helper;
using FakeApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace FakeApi.Services
{
	public class TouristRouteRepository : ITouristRouteRepository
	{
		private readonly AppDbContext _context;
		private readonly IPropertyMappingService _propertyMappingService;

		public TouristRouteRepository(AppDbContext context,IPropertyMappingService propertyMappingService)
		{
			_context = context;
			_propertyMappingService = propertyMappingService;
		}

		public async Task<TouristRoute> GetTouristRouteAsync(Guid touristRouteId)
		{
			return await _context.TourisRoutes.Include(t => t.TouristRoutePictures).FirstOrDefaultAsync(n => n.Id == touristRouteId);
		}

		public async Task<PaginationList<TouristRoute>> GetTouristRoutesAsync(
			string keyword,
			string ratingOperator,
			int? ratingValue,
			int pageSize,
			int pageNumber,
			string orderBy
		)
		{
			// Include 和 join(延遲加載) 可以合併表格
			// IQueryable 可以疊加處理linq (生成sql語法)
			IQueryable<TouristRoute> result = _context.TourisRoutes.Include(t => t.TouristRoutePictures);
			if (!string.IsNullOrWhiteSpace(keyword)) // keyword 不等於 null or ""
			{
				keyword = keyword.Trim();
				result = result.Where(t => t.Title.Contains(keyword));
			}
			if (ratingValue >= 0)
			{
				result = ratingOperator switch
				{
					"largerThan" => result.Where(t => t.Rating >= ratingValue),
					"lessThan" => result.Where(t => t.Rating <= ratingValue),
					_ => result.Where(t => t.Rating == ratingValue),
				};
			}

			//// pageination // 用pagination工廠替代
			//// skip
			//var skip = (pageNumber - 1) * pageSize;
			//result = result.Skip(skip);
			//// 以pagesize為標準顯示一定量的數據
			//result = result.Take(pageSize);
			//return await result.ToListAsync()

			if (!string.IsNullOrWhiteSpace(orderBy)) // keyword 不等於 null or ""
			{
				if(orderBy.ToLowerInvariant() == "originalprice") // 轉換字串為小寫
				{
					result = result.OrderBy(t => t.OriginalPrice);	
				}

				// 獲取旅遊路線的映射字典
				var touristRouteMappingDictionary = _propertyMappingService
					.GetPropertyMapping<TouristRouteDto,TouristRoute>();

				result = result.ApplySort(orderBy/*API接收到的排序字符串*/, touristRouteMappingDictionary);
			}
			return await PaginationList<TouristRoute>.CreateAsync(pageNumber, pageSize, result);
		}

		public async Task<IEnumerable<TouristRoute>> GetTouristRoutesByIDListAsync(IEnumerable<Guid> ids)
		{
			return await _context.TourisRoutes.Where(t => ids.Contains(t.Id)).ToListAsync();
		}

		public async Task<bool> TouristRouteExistsAsync(Guid touristRouteId)
		{
			return await _context.TourisRoutes.AnyAsync(t => t.Id == touristRouteId);
		}

		public async Task<IEnumerable<TouristRoutePicture>> GetPicturesByTouristRouteIdAsync(Guid touristRouteId)
		{
			return await _context.TouristRoutePictures.Where(p => p.TouristRouteId == touristRouteId).ToListAsync();
		}

		public async Task<TouristRoutePicture> GetPictureAsync(int pictureId)
		{
			return await _context.TouristRoutePictures.Where(p => p.Id == pictureId).FirstOrDefaultAsync();
		}

		public void AddTouristRoute(TouristRoute touristRoute)
		{
			if (touristRoute == null)
			{
				throw new ArgumentNullException(nameof(touristRoute));
			}
			_context.TourisRoutes.Add(touristRoute);
			//_context.SaveChanges();
		}

		public void AddTouristRoutePicture(Guid touristRouteId, TouristRoutePicture touristRoutePicture)
		{
			if (touristRouteId == Guid.Empty)
			{
				throw new ArgumentNullException(nameof(touristRouteId));
			}
			if (touristRoutePicture == null)
			{
				throw new ArgumentNullException(nameof(touristRoutePicture));
			}
			touristRoutePicture.TouristRouteId = touristRouteId;
			_context.TouristRoutePictures.Add(touristRoutePicture);
		}

		public void DeleteTouristRoute(TouristRoute touristRoute)
		{
			_context.TourisRoutes.Remove(touristRoute);
		}

		public void DeleteTouristRoutePicture(TouristRoutePicture touristRoutePicture)
		{
			_context.TouristRoutePictures.Remove(touristRoutePicture);
		}

		public void DeleteTouristRoutes(IEnumerable<TouristRoute> touristRoutes)
		{
			_context.TourisRoutes.RemoveRange(touristRoutes);
		}

		public async Task<ShoppingCart> GetShoppingCartByUserId(string userId)
		{
			return await _context.ShoppingCarts
				.Include(s => s.User) // 將 ShoppingCart 表和 ApplicationUser 表進行連接
				.Include(s => s.ShoppingCartItems).ThenInclude(li => li.TouristRoute) // 將 ShoppingCart 表和 LineItem 表進行連接 再將 LineItem 和 TouristRoute 連接
				.Where(s => s.UserId == userId) // 過濾使用者ID
				.FirstOrDefaultAsync();
		}

		public async Task CreateShoppingCart(ShoppingCart shoppingCart)
		{
			await _context.ShoppingCarts.AddAsync(shoppingCart);
		}

		public async Task AddShoppingCartItem(LineItem lineItem)
		{
			await _context.LineItems.AddAsync(lineItem);
		}

		public async Task<LineItem> GetShoppingCartItemByItemId(int lineItemId)
		{
			return await _context.LineItems.Where(li => li.Id == lineItemId).FirstOrDefaultAsync();
		}

		public void DeleteShoppingCartItem(LineItem lineItem)
		{
			_context.LineItems.Remove(lineItem);
		}

		public async Task<IEnumerable<LineItem>> GetShoppingCartsByIdListAsync(IEnumerable<int> ids)
		{
			return await _context.LineItems.Where(li => ids.Contains(li.Id)).ToListAsync();
		}

		public void DeleteShoppingCartItems(IEnumerable<LineItem> lineItems)
		{
			_context.LineItems.RemoveRange(lineItems);
		}

		public async Task AddOrderAsync(Order order)
		{
			await _context.Orders.AddAsync(order);
		}
		public async Task<PaginationList<Order>> GetOrderByUserId(string userId, int pageSize, int pageNumber)
		{
			//return await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
			IQueryable<Order> result = _context.Orders.Where(o => o.UserId == userId);
			return await PaginationList<Order>.CreateAsync(pageNumber, pageSize, result);
		}
		public async Task<Order> GetOrderById(Guid orderId)
		{
			return await _context.Orders
				.Include(o => o.OrderItems) // 獲取訂單內產品列表
				.ThenInclude(oi => oi.TouristRoute) // 連接旅遊路線資訊
				.Where(o => o.Id == orderId).FirstOrDefaultAsync();
		}

		public async Task<bool> SaveAsync()
		{
			return (await _context.SaveChangesAsync() >= 0);
		}

		
	}
}
