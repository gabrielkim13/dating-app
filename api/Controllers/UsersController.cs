using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using api.Data;
using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Helpers;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
  [Authorize]
  public class UsersController : BaseApiController
  {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {
      _unitOfWork = unitOfWork;
      _photoService = photoService;
      _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers([FromQuery] UserParams userParams)
    {
      var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());

      userParams.CurrentUsername = User.GetUsername();

      if (string.IsNullOrEmpty(userParams.Gender))
        userParams.Gender = gender == "male" ? "female" : "male";

      var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

      Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

      return Ok(users);
    }

    [HttpGet("{username}", Name = "GetUser")]
    public async Task<ActionResult<MemberDTO>> GetUser(string username)
    {
      return await _unitOfWork.UserRepository.GetMemberAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
    {
      var username = User.GetUsername();
      var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

      _mapper.Map(memberUpdateDTO, user);

      if (await _unitOfWork.Complete()) return NoContent();

      return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
    {
      var username = User.GetUsername();
      var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

      var result = await _photoService.AddPhotoAsync(file);

      if (result.Error != null) return BadRequest(result.Error.Message);

      var photo = new Photo
      {
        Url = result.SecureUrl.AbsoluteUri,
        PublicId = result.PublicId,
      };

      if (user.Photos.Count == 0) photo.IsMain = true;

      user.Photos.Add(photo);

      if (await _unitOfWork.Complete())
      {
        return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDTO>(photo));
      }

      return BadRequest("Problem uploading photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
      var username = User.GetUsername();
      var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

      var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

      if (photo.IsMain) return BadRequest("This is already your main photo");

      photo.IsMain = true;

      var currentMainPhoto = user.Photos.FirstOrDefault(photo => photo.IsMain);

      if (currentMainPhoto != null) currentMainPhoto.IsMain = false;

      if (await _unitOfWork.Complete()) return NoContent();

      return BadRequest("Failed to set main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
      var username = User.GetUsername();
      var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

      var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

      if (photo == null) return NotFound();

      if (photo.IsMain) return BadRequest("You cannot delete your main photo");

      if (photo.PublicId != null)
      {
        var result = await _photoService.DeletePhotoAsync(photo.PublicId);

        if (result.Error != null) return BadRequest(result.Error.Message);
      }

      user.Photos.Remove(photo);

      if (await _unitOfWork.Complete()) return NoContent();

      return BadRequest("Failed to delete photo");
    }
  }
}
