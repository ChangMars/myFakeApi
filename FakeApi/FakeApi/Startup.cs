using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeApi.Database;
using FakeApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using FakeApi.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FakeApi
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services) // 用來管理組件依賴
		{
			services.AddIdentity<ApplicationUser, IdentityRole>(/*option => option.SignIn.RequireConfirmedAccount = false*/)
				.AddEntityFrameworkStores<AppDbContext>();

			// JWT 驗證依賴服務注入
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(option =>
				{
					var secretByte = Encoding.UTF8.GetBytes(Configuration["Authentication:SecretKey"]);
					option.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidIssuer = Configuration["Authentication:Issuer"],

						ValidateAudience = true,
						ValidAudience = Configuration["Authentication:Audience"],

						ValidateLifetime = true,

						IssuerSigningKey = new SymmetricSecurityKey(secretByte)
					};
				});
			/*
			 * 三層架構 DAL->BLL->UI
			 */
			/* 
			 * MVC架構優點:
			 * 耦合性低
			 * 高可復用性
			 * 高可維護性
			 * MVC架構缺點:
			 * 定義不明確，學習曲線難
			 * 結構複雜
			 * 數據流動效率低
			 */
			services.AddControllers(setupAction => // 註冊MVC控制器
			{
				setupAction.ReturnHttpNotAcceptable = true; // 限定Accept格式
				//setupAction.OutputFormatters.Add(
				//	new XmlDataContractSerializerOutputFormatter()
				//);
			})
			.AddNewtonsoftJson(setupAction => // 配置Newtonsoft 用於 JSON 轉 JSONPatch
			{
				setupAction.SerializerSettings.ContractResolver =
					new CamelCasePropertyNamesContractResolver();
			})
			.AddXmlDataContractSerializerFormatters() // 用於配置xml所有output 和 input
			//透過 ConfigureApiBehaviorOptions() 來修改 MVC 內用來產生模型繫結回應的工廠物件
			//InvalidModelStateResponseFactory，你可以自訂當發生模型繫結錯誤時，你要做甚麼動作，
			//以及要回傳怎樣的訊息給呼叫端。
			.ConfigureApiBehaviorOptions(setupAction =>
			{
				setupAction.InvalidModelStateResponseFactory = context => // 非法模型狀態響應工廠
				{
					// 新增Error 422 錯誤
					var problemDetail = new ValidationProblemDetails(context.ModelState)
					{
						Type = "無所謂",
						Title = "數據驗證失敗",
						Status = StatusCodes.Status422UnprocessableEntity,
						Detail = "請看詳細說明",
						Instance = context.HttpContext.Request.Path
					};
					// 新增追蹤ID
					problemDetail.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
					return new UnprocessableEntityObjectResult(problemDetail) // 用422代替400
					{
						ContentTypes = { "application/problem+json" }
					};
				};
			});

			services.AddTransient<ITouristRouteRepository, TouristRouteRepository>();
			//services.AddSingleton
			//services.AddScoped
			//services.AddEntityFrameworkNpgsql().AddDbContext<AppDbContext>(options => options.UseNpgsql(Configuration["PostgresDbContext:ConnectionString"]));
			services.AddDbContext<AppDbContext>(option =>
			{
				//option.UseSqlServer("server=localhost; Database=FakeDb; User Id=sa; Password=PaSSword12!");
				//option.UseSqlServer(Configuration["DbContext:ConnectionString"]);
				option.UseNpgsql(Configuration["GCPPostgresDbContext:ConnectionString"]);
			});

			// 加入 automapper扫描 profile 文件
			services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

			services.AddHttpClient(); // 新增http請求服務

			// 專門用來管理URL配置組件
			// 註冊IUrlHepper服務
			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

			// 注入排序功能組件
			services.AddTransient<IPropertyMappingService, PropertyMappingService>();

			// 註冊自定義媒體類型(格式處理器)(全局添加)
			services.Configure<MvcOptions>(config =>
			{
				// 保存格式處理器
				var outputFormatter = config.OutputFormatters // 所有媒體處理器列表
					.OfType<NewtonsoftJsonOutputFormatter>()?
					.FirstOrDefault();
				if (outputFormatter != null)
				{
					outputFormatter.SupportedMediaTypes // 加入自定義媒體類型
						.Add("application/vnd.rong.hateoas+json");
				}
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env) // 用來配置系統HTTP請求通道
		{
			/*
			 * 檢查&處理 http 請求
			 * 交由中間件處理(middelware) 
			 * 請求通道是透過IApplicationBuilder創建
			 * 中間件必須在Endpoints
			 * 
			 */
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting(); // 妳在哪

			app.UseAuthentication(); // 妳是誰

			app.UseAuthorization(); // 妳可以幹什麼，有什麼權限

			app.UseEndpoints(endpoints =>
			{
				//endpoints.MapGet("/test", async context =>
				//{
				//	throw new Exception("test");
				//	await context.Response.WriteAsync("Hello from test!");
				//});
				//endpoints.MapGet("/", async context =>
				//{
				//	await context.Response.WriteAsync("Hello World!");
				//});
				endpoints.MapControllers();
			});
		}
	}
}
