using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class UsersController : BaseApiController
{
    private readonly DataContext _context;

    public UsersController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsersAsync()
        => await _context.Users.ToListAsync(HttpContext.RequestAborted);

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(new object?[] {id}, cancellationToken: HttpContext.RequestAborted);

        if (user == null)
        {
            return NotFound();
        }

        return new JsonResult(user);
    }
}