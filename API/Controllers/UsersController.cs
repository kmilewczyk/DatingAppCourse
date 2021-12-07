using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsersAsync()
    {
        var membersAsync = await _userRepository.GetMembersAsync();
        var membersToReturn = _mapper.Map<IEnumerable<MemberDto>>(membersAsync);
        return Ok(membersToReturn);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult> GetUser(string username)
    {
        var member = await _userRepository.GetMemberAsync(username);

        if (member == null)
        {
            return NotFound();
        }

        return new JsonResult(_mapper.Map<MemberDto>(member));
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userRepository.GetUserByUsernameAsync(username);

        _mapper.Map(memberUpdateDto, user);
        
        _userRepository.Update(user);

        if (await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Failed to update user.");
    }
}