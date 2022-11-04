using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FakeApi.Dtos;
using FakeApi.Models;

namespace FakeApi.Profiles
{
	public class TouristRouteProfile : Profile
	{
		// 用於 Auto Mapper 自動映射
		// 相同名稱自動映射
		public TouristRouteProfile()
		{
			// 新增 TouristRoute 和 TouristRouteDto 映射
			CreateMap<TouristRoute,TouristRouteDto>()
				.ForMember(
					dest => dest.Price,
					opt => opt.MapFrom(src => src.OriginalPrice * (decimal)(src.DiscountPresent ?? 1))
				)
				.ForMember(
					dest => dest.TravelDays,
					opt => opt.MapFrom(src => src.TravelDays.ToString())
				)
				.ForMember(
					dest => dest.TripType,
					opt => opt.MapFrom(src => src.TripType.ToString())
				)
				.ForMember(
					dest => dest.DepartureCity,
					opt => opt.MapFrom(src => src.DepartureCity.ToString())
				);
			// 新增 TouristRouteForCreateionDto 和 TouristRoute 映射
			CreateMap<TouristRouteForCreateionDto, TouristRoute>()
				.ForMember(
					dest => dest.Id,
					opt => opt.MapFrom(src => Guid.NewGuid())
				);
			// 新增 TouristRouteUpdateDto 和 TouristRoute 映射 
			CreateMap<TouristRouteForUpdateDto, TouristRoute>();
			// 新增 TouristRoute 和 TouristRouteForUpdateDto 映射 
			CreateMap<TouristRoute, TouristRouteForUpdateDto>();
			// 新增 TouristRoute 和 TouristRouteSimplifyDto 映射
			CreateMap<TouristRoute, TouristRouteSimplifyDto>()
				.ForMember(
					dest => dest.Price,
					opt => opt.MapFrom(src => src.OriginalPrice * (decimal)(src.DiscountPresent ?? 1))
				);
		}
	}
}
