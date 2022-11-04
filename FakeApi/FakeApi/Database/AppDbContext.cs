using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeApi.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Policy;

namespace FakeApi.Database
{
	public class AppDbContext : IdentityDbContext<ApplicationUser> //DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options)
			: base(options)
		{

		}

		public DbSet<TouristRoute> TourisRoutes { get; set; }

		public DbSet<TouristRoutePicture> TouristRoutePictures { get; set; }

		public DbSet<ShoppingCart> ShoppingCarts { get; set; }

		public DbSet<LineItem> LineItems { get; set; }

		public DbSet<Order> Orders { get; set; }

		// 用來控制數據庫和模型映射
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//modelBuilder.Entity<TouristRoute>().HasData(new TouristRoute()
			//{
			//	Id = Guid.NewGuid(),
			//	Title = "測試標頭",
			//	Description ="shouming",
			//	OriginalPrice=0,
			//	CreateTime=DateTime.UtcNow
			//});

			// Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location 獲取本地資料夾地址
			var touristRouteJsonData = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/Database/touristRoutesMockData.json");
			IList<TouristRoute> touristRoutes = JsonConvert.DeserializeObject<IList<TouristRoute>>(touristRouteJsonData);
			modelBuilder.Entity<TouristRoute>().HasData(touristRoutes);

			var touristRoutePictureJsonData = File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"/Database/touristRoutePicturesMockData.json");
			IList<TouristRoutePicture> touristRoutePictures = JsonConvert.DeserializeObject<IList<TouristRoutePicture>>(touristRoutePictureJsonData);
			modelBuilder.Entity<TouristRoutePicture>().HasData(touristRoutePictures);

			// 初始化用戶與角色的基本數據
			// 1.更新用戶與角色的外建
			modelBuilder.Entity<ApplicationUser>(u =>
				u.HasMany(x => x.UserRoles)
				.WithOne().HasForeignKey(ur => ur.UserId).IsRequired()
			);

			// 2.添加管理員角色
			var adminRoleId = "308660dc-ae51-480f-824d-7dca6714c3e2";
			modelBuilder.Entity<IdentityRole>().HasData(
				new IdentityRole()
				{
					Id = adminRoleId,
					Name = "Admin",
					NormalizedName = "Admin".ToUpper()
				}
			);

			// 3.添加用戶
			var adminUserId = "90184155-dee0-40c9-bb1e-b5ed07afc04e";
			ApplicationUser adminUser = new ApplicationUser
			{
				Id = adminUserId,
				UserName = "admin@fakexiecheng.com",
				NormalizedUserName = "admin@fakexiecheng.com".ToUpper(),
				Email = "admin@fakexiecheng.com",
				NormalizedEmail = "admin@fakexiecheng.com".ToUpper(),
				TwoFactorEnabled = false,
				EmailConfirmed = true,
				PhoneNumber = "123456789",
				PhoneNumberConfirmed = false
			};
			PasswordHasher<ApplicationUser> ph = new PasswordHasher<ApplicationUser>();
			adminUser.PasswordHash = ph.HashPassword(adminUser, "Fake123$");
			modelBuilder.Entity<ApplicationUser>().HasData(adminUser);

			// 4.給用戶加入管理員角色
			modelBuilder.Entity<IdentityUserRole<string>>().HasData(
				new IdentityUserRole<string>()
				{
					RoleId = adminRoleId,
					UserId = adminUserId
				}
			);


			base.OnModelCreating(modelBuilder);
		}
	}
}
