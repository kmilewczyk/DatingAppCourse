using System.Security.Claims;

namespace API.Extension;

public static class ClaimsPrincipalExtensions
{
    public static string GetUsername(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Name)?.Value;

    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (value != null)
            return int.Parse(value);

        return null;
    }
}