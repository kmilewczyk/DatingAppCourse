using API.DTOs;
using API.Entities;
using API.Extension;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;

    public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsersAsync([FromQuery] UserParams userParams)
    {
        var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
        userParams.CurrentUsername = User.GetUsername();
        userParams.CurrentUserId = User.GetUserId()!.Value;

        if (string.IsNullOrEmpty(userParams.Gender))
            userParams.Gender = gender == "male" ? "female" : "male";

        var members = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(members.CurrentPage, members.PageSize, members.TotalCount, members.TotalPages);

        return Ok(members);
    }

    [HttpGet("{username}", Name = "GetUser")]
    public async Task<ActionResult> GetUser(string username)
    {
        var member = await _unitOfWork.UserRepository.GetMemberAsync(username);

        if (member == null)
        {
            return NotFound();
        }

        return new JsonResult(_mapper.Map<MemberDto>(member));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        _mapper.Map(memberUpdateDto, user);

        _unitOfWork.UserRepository.Update(user);

        if (await _unitOfWork.Complete()) return NoContent();

        return BadRequest("Failed to update user.");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error is not null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        // User doesn't have any photos in the collection
        if (user.Photos.Count == 0)
        {
            photo.IsMain = true;
        }

        user.Photos.Add(photo);

        if (await _unitOfWork.Complete())
        {
            return CreatedAtRoute("GetUser", new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
        }

        return BadRequest("Problem adding photo.");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo is null) return NotFound("Photo of id {photoId} doesn't exist.");

        if (photo.IsMain) return BadRequest("This is already your main photo.");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

        if (currentMain != null) currentMain.IsMain = false;

        photo.IsMain = true;

        if (await _unitOfWork.Complete()) return NoContent();

        return BadRequest("failed to set main photo.");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

        var photo = user.Photos.FirstOrDefault(u => u.Id == photoId);

        if (photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("You cannot delete your main photo");

        if (photo.PublicId != null)
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await _unitOfWork.Complete()) return Ok();

        return BadRequest("Failed to delete the photo.");
    }
}