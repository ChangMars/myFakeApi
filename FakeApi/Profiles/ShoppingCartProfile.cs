using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FakeApi.Dtos;
using FakeApi.Models;

namespace FakeApi.Profiles
{
	public class ShoppingCartProfile : Profile
	{
		public ShoppingCartProfile()
		{
			CreateMap<ShoppingCart, ShoppingCartDto>();
			CreateMap<LineItem, LineItemDto>();
		}
	}
}
