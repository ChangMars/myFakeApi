using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FakeApi.Helper
{
	public class PaginationList<T> : List<T>
	{
		public int TotalPages { get; private set; }
		public int TotalCount { get; private set; }
		public bool HasPrevious => CurrentPage > 1;
		public bool HasNext => CurrentPage < TotalPages;
		public int CurrentPage { get; set; }
		public int PageSize { get; set; }
		public PaginationList(int totalCount ,int currentPage,int pageSize,List<T> items)
		{
			CurrentPage = currentPage;
			PageSize = pageSize;
			AddRange(items);
			TotalCount = totalCount;
			// Math.Ceiling =  傳回大於或等於指定數字的最小整數值 = 無條件進位
			TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize); 
		}

		// 使用工廠設計模式
		public static async Task<PaginationList<T>> CreateAsync(int currentPage, int pageSize ,IQueryable<T> result)
		{
			var totalCount = await result.CountAsync(); // 會進行數據庫 操作
			// pageination
			// skip
			var skip = (currentPage - 1) * pageSize;
			result = result.Skip(skip);
			// 以pagesize為標準顯示一定量的數據
			result = result.Take(pageSize);

			var items = await result.ToListAsync();
			return new PaginationList<T>(totalCount, currentPage, pageSize, items);
		}
	}
}
