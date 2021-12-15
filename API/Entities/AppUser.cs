using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class AppUser : IdentityUser<int>
{
    private DateTime _dateOfBirth;
    private DateTime _created = DateTime.UtcNow;
    private DateTime _lastActive = DateTime.UtcNow;

    public DateTime DateOfBirth
    {
        get => _dateOfBirth;
        set => _dateOfBirth = value.ToUniversalTime();
    }

    public DateTime Created
    {
        get => _created;
        set => _created = value.ToUniversalTime();
    }

    public DateTime LastActive
    {
        get => _lastActive;
        set => _lastActive = value.ToUniversalTime();
    }

    public string KnownAs { get; set; }
    public string Gender { get; set; }
    public string Introduction { get; set; }
    public string LookingFor { get; set; }
    public string Interests { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public ICollection<Photo> Photos { get; set; }
    public ICollection<UserLike> LikedByUsers { get; set; }
    public ICollection<UserLike> LikedUsers { get; set; }
    public ICollection<Message> MessagesSent { get; set; }
    public ICollection<Message> MessagesReceived { get; set; }
    public ICollection<AppUserRole> UserRoles { get; set; }
}