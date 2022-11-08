using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FakeApi.Dtos;
using FakeApi.Models;

namespace FakeApi.Profiles
{
	public class TouristRoutePictureProfile : Profile
	{
		public TouristRoutePictureProfile()
		{
			CreateMap<TouristRoutePicture, TouristRoutePictureDto>();
			CreateMap<TouristRoutePictureForCreationDto, TouristRoutePicture>();
			CreateMap<TouristRoutePicture, TouristRoutePictureForCreationDto>();
		}
	}
}
