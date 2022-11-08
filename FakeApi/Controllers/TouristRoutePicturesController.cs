using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FakeApi.Dtos;
using FakeApi.Models;
using FakeApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FakeApi.Controllers
{
    [Route("api/touristRoutes/{touristRouteId}/pictures")] 
    [ApiController]
	public class TouristRoutePicturesController : ControllerBase
	{
        private ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;

        public TouristRoutePicturesController(ITouristRouteRepository touristRouteRepository, IMapper mapper)
        {
            _touristRouteRepository = touristRouteRepository ?? 
                throw new ArgumentNullException(nameof(touristRouteRepository));
            _mapper = mapper ?? 
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<IActionResult> GetPictureListForTouristRoute(Guid touristRouteId)
		{
            if(!await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))
			{
                return NotFound("旅遊路線不存在");
			}
            var picturesFromRepo = await _touristRouteRepository.GetPicturesByTouristRouteIdAsync(touristRouteId);
            if(picturesFromRepo == null || picturesFromRepo.Count() <= 0)
            {
                return NotFound("旅遊路線不存在");
            }
            return Ok(_mapper.Map<IEnumerable<TouristRoutePictureDto>>(picturesFromRepo));
        }

        [HttpGet("{pictureId}", Name = "GetPicture")]
        public async Task<IActionResult> GetPicture(Guid touristRouteId,int pictureId)
        {
            // 先判斷父資源存不存在
            if (!await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅遊路線不存在");
            }

            var picturesFromRepo = await _touristRouteRepository.GetPictureAsync(pictureId);
            if (picturesFromRepo == null)
            {
                return NotFound("旅遊路線不存在");
            }
            return Ok(_mapper.Map<TouristRoutePictureDto>(picturesFromRepo));
        }

        [HttpPost(Name = "CreateTouristRoutePicture")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTouristRoutePicture(
            [FromRoute]Guid touristRouteId,
            [FromBody] TouristRoutePictureForCreationDto touristRoutePictureForCreationDto)
		{
            // 先判斷父資源存不存在
            if (!await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅遊路線不存在");
            }

            var pictureModel = _mapper.Map<TouristRoutePicture>(touristRoutePictureForCreationDto);
            _touristRouteRepository.AddTouristRoutePicture(touristRouteId, pictureModel);
            await _touristRouteRepository.SaveAsync();
            var pictureToReturn = _mapper.Map<TouristRoutePictureDto>(pictureModel);
            return CreatedAtRoute(
                "GetPicture",
                new
                {
                    touristRouteId = pictureModel.TouristRouteId,
                    pictureId = pictureModel.Id
                },
                pictureToReturn
            );
        }

        [HttpDelete("{pictureId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePicture(
            [FromRoute] Guid touristRouteId,
            [FromRoute] int pictureId
        )
		{
            if (!await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))
            {
                return NotFound("旅遊路線不存在");
            }
            var picture = await _touristRouteRepository.GetPictureAsync(pictureId);
            _touristRouteRepository.DeleteTouristRoutePicture(picture);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }
    }
}
