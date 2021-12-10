using System.Linq;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void Update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users
            .Include(user => user.Photos)
            .ToListAsync();
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(user => user.Photos)
            .SingleOrDefaultAsync(user => user.UserName == username);
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        var query = _context.Users.AsQueryable();

        // Filter out current user
        query = query.Where(u => u.UserName != userParams.CurrentUsername);
        
        // Filter by gender
        query = query.Where(u => u.Gender == userParams.Gender);

        // Filter by Date of Birth range
        var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
        var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
        query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

        // Aggregate now count of all users who match the criteria
        var count = await query.CountAsync();

        // Start on retrieving users with pagination
        query = query.Include(src => src.Photos);
        
        // Sort by filter criteria
        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive)
        };
        
        // Paginate
        query = query.Skip((userParams.PageNumber - 1) * userParams.PageSize).Take(userParams.PageSize);

        // NOTE: joining and union cannot be done in pure LINQ due to https://github.com/dotnet/efcore/issues/16243
        
        var memberDtoQuery = query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking();

        // Get members that were liked by the current user.
        var likedMemberDtoQuery = query
            .Join(
                _context.Likes,
                user => user.Id,
                like => like.LikedUserId,
                (user, like) => new { user, like })
            .Where(join => join.like.SourceUserId == userParams.CurrentUserId)
            .Select(join => MapLeftJoinToMemberDto(join.user, true, _mapper));

        var likedMemberDto = await likedMemberDtoQuery.AsNoTracking().ToListAsync();

        // Join the two sets and sort them
        var list = (await memberDtoQuery.ToListAsync())
            .Concat(likedMemberDto)
            .OrderBy(m => m.Id);
        
        // Return a paged list, but first remove duplicates that came from memberDtoQuery.
        return new PagedList<MemberDto>(FilterJoinDuplicates(list), count, userParams.PageNumber, userParams.PageSize);
    }

    public async Task<MemberDto> GetMemberAsync(string username)
    {
        return await _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    private static MemberDto AddLikedToMemberDto(MemberDto member, bool liked)
    {
        member.Liked = liked;
        return member;
    }

    private static MemberDto MapLeftJoinToMemberDto(AppUser user, bool liked, IMapper mapper)
    {
        var member = mapper.Map<MemberDto>(user);
        member.Liked = liked;

        return member;
    }

    /// <summary>
    /// Filters our duplicates. Chooses the duplicate with Liked field set to true.
    /// </summary>
    /// <param name="list">Ordered list</param>
    /// <returns>Unique list with members have set Liked to true if they were liked.</returns>
    private IEnumerable<MemberDto> FilterJoinDuplicates(IEnumerable<MemberDto> list)
    {
        MemberDto currentMember = null;
        foreach (var member in list)
        {
            // If it is the first iteration
            if (currentMember == null)
            {
                currentMember = member;
            }
            // If this iteration's member is a duplicate of the previous iteration
            else if (currentMember.Id == member.Id)
            {
                currentMember = member.Liked ? member : currentMember;
            }
            // If this iteration's member is different from the previous iteration
            else
            {
                yield return currentMember;
                currentMember = member;
            }
        }

        yield return currentMember;
    }
}