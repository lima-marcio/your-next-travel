using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace YourNextTravel.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        return Guid.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
    }
}
