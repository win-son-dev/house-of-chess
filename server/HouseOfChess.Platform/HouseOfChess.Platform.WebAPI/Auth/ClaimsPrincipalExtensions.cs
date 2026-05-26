using System.Security.Claims;

namespace HouseOfChess.Platform.WebAPI.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string? GetAuth0Sub(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.NameIdentifier);
}
