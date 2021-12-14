using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await _userManager.Users
            .Include(r => r.UserRoles)
            .ThenInclude(r => r.Role)
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
    {
        username = username.ToLowerInvariant();
        
        var selectedRoles = roles.Split(",").ToArray();

        var user = await _userManager.FindByNameAsync(username);

        if (user == null) return NotFound("Could not find user");

        var userRoles = await _userManager.GetRolesAsync(user);

        var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

        if (!result.Succeeded) return BadRequest("Failed to add roles to the user");

        result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

        if (!result.Succeeded) return BadRequest("Failed to remove user from the roles.");

        return Ok(await _userManager.GetRolesAsync(user));
    }
    
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("unapproved-photos")]
    public async Task<ActionResult<IEnumerable<PhotoForModerationDto>>> GetPhotosForModeration()
    {
        return Ok(await _unitOfWork.PhotoRepository.GetUnapprovedPhotosAsync());
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("unapproved-photos/{id}/approve")]
    public async Task<ActionResult> ApprovePhoto(int id)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhoto(id);

        if (photo == null)
        {
            return NotFound();
        }

        await _unitOfWork.PhotoRepository.ApprovePhotoAsync(photo);

        var user = await _unitOfWork.UserRepository.GetUserWithPhotosAsync(photo.UserId);
        if (!user.Photos.Any(p => p.IsMain))
        {
            photo.IsMain = true;
        }

        if (await _unitOfWork.Complete()) 
            return Ok();

        return BadRequest("Failed to approve the photo");
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("unapproved-photos/{id}/reject")]
    public async Task<ActionResult> RejectPhoto(int id)
    {
        var photo = await _unitOfWork.PhotoRepository.GetPhoto(id);

        if (photo == null || photo.Approved)
            return NotFound();

        await _unitOfWork.PhotoRepository.RejectPhotoAsync(photo);

        if (await _unitOfWork.Complete())
            return Ok();

        return BadRequest("Failed to reject the photo.");
    }
}