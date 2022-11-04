using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FakeApi.Dtos;
using FakeApi.Models;

namespace FakeApi.Profiles
{
	public class OrderProfile : Profile
	{
		public OrderProfile()
		{
			CreateMap<Order, OrderDto>() // 枚舉類型不能自動映射 所以要配置
				.ForMember(
					dest => dest.State,
					opt =>
					{
						opt.MapFrom(src => src.State.ToString());
					}
				);
		}
	}
}
