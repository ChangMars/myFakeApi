
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FakeApi.Dtos;
using FakeApi.Models;
using FakeApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FakeApi.Controllers
{
	[ApiController]
	[Route("auth")]
	public class AuthenticateController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private ITouristRouteRepository _touristRouteRepository;

		public AuthenticateController(
			IConfiguration configuration,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			ITouristRouteRepository touristRouteRepository)
		{
			_configuration = configuration;
			_userManager = userManager;
			_signInManager = signInManager;
			_touristRouteRepository = touristRouteRepository;
		}

		[AllowAnonymous]
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
		{
			// 1. 驗證用戶密碼
			var loginResult = await _signInManager.PasswordSignInAsync(
				loginDto.Email,
				loginDto.Password,
				false,
				false
			);

			if(!loginResult.Succeeded)
			{
				return BadRequest();
			}

			var user = await _userManager.FindByNameAsync(loginDto.Email);
			// 2. 創建JWT
			// header
			var signigAlgoritm = SecurityAlgorithms.HmacSha256;
			// payload
			var claims = new List<Claim> 
			{
				// sub
				new Claim(JwtRegisteredClaimNames.Sub, user.Id),
				new Claim(ClaimTypes.Role, "Admin")
			};
			var roleNames = await _userManager.GetRolesAsync(user);
			foreach(var roleName in roleNames)
			{
				var roleClaim = new Claim(ClaimTypes.Role, roleName);
				claims.Add(roleClaim);
			}
			// signiture
			var secretByte = Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]); //密鑰長度要夠長不然會出錯
			var signingKey = new SymmetricSecurityKey(secretByte);
			var signingCredentials = new SigningCredentials(signingKey, signigAlgoritm);

			var token = new JwtSecurityToken(
					issuer: _configuration["Authentication:Issuer"],
					audience: _configuration["Authentication:Audience"],
					claims,
					notBefore: DateTime.UtcNow,
					expires: DateTime.UtcNow.AddDays(1),
					signingCredentials
				);

			var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
			// 3. return 200 ok jwt
			return Ok(tokenStr);
		}

		[AllowAnonymous]
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
		{
			// 1. 使用用戶名創建用戶對象
			var user = new ApplicationUser()
			{
				UserName = registerDto.Email,
				Email = registerDto.Email
			};
			// 2. hash密碼,保存用戶
			var result = await _userManager.CreateAsync(user, registerDto.Password);
			if(!result.Succeeded)
			{
				return BadRequest();
			}
			// 3. 初始化購物車
			var shoppingCart = new ShoppingCart()
			{
				Id = Guid.NewGuid(),
				UserId = user.Id,
			};
			await _touristRouteRepository.CreateShoppingCart(shoppingCart);
			await _touristRouteRepository.SaveAsync();

			// 4. return
			return Ok();
		}
	}
}
