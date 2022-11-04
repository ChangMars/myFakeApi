using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeApi.Helper;
using FakeApi.Models;
using Microsoft.AspNetCore.Identity;

namespace FakeApi.Services
{
	public interface ITouristRouteRepository
	{
		Task<TouristRoute> GetTouristRouteAsync(Guid touristRouteId);

		//Task<IEnumerable<TouristRoute>> GetTouristRoutesAsync(string keyword, string ratingOperator, int? ratingValue, int pageSize, int pageNumber);
		// 將原始的列表 套用分頁工廠模式 來產生數據 
		Task<PaginationList<TouristRoute>> GetTouristRoutesAsync(string keyword, string ratingOperator, 
			int? ratingValue, int pageSize, int pageNumber,
			string orderBy);
		Task<IEnumerable<TouristRoute>> GetTouristRoutesByIDListAsync(IEnumerable<Guid> ids);

		Task<bool> TouristRouteExistsAsync(Guid touristRouteId);

		Task<TouristRoutePicture> GetPictureAsync(int pictureId);
		Task<IEnumerable<TouristRoutePicture>> GetPicturesByTouristRouteIdAsync(Guid touristRouteId);

		void AddTouristRoute(TouristRoute touristRoute);
		void AddTouristRoutePicture(Guid touristRouteId, TouristRoutePicture touristRoutePicture);

		void DeleteTouristRoute(TouristRoute touristRoute);
		void DeleteTouristRoutePicture(TouristRoutePicture touristRoutePicture);
		void DeleteTouristRoutes(IEnumerable<TouristRoute> touristRoutes);

		Task<ShoppingCart> GetShoppingCartByUserId(string userId);
		Task CreateShoppingCart(ShoppingCart shoppingCart);
		Task AddShoppingCartItem(LineItem lineItem);
		Task<LineItem> GetShoppingCartItemByItemId(int lineItemId);
		void DeleteShoppingCartItem(LineItem lineItem);
		Task<IEnumerable<LineItem>> GetShoppingCartsByIdListAsync(IEnumerable<int> ids);
		void DeleteShoppingCartItems(IEnumerable<LineItem> lineItems);

		Task AddOrderAsync(Order order);
		Task<PaginationList<Order>> GetOrderByUserId(string userId, int pageSize, int pageNumber);
		Task<Order> GetOrderById(Guid orderId);

		Task<bool> SaveAsync();
	}
}
