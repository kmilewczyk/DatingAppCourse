using API.DTOs;
using API.DTOs.Member;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepository
{
    void Update(AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser> FindUserAsync(int id);
    Task<AppUser> GetUserWithPhotosAsync(int id);
    Task<AppUser> GetUserByUsernameAsync(string username);
    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto> GetMemberAsync(string username);
    Task<ProfileMemberDto?> GetCurrentMemberAsync(string username);
    Task<string> GetUserGender(string username);
}