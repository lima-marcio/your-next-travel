namespace YourNextTravel.Api.Features.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string SigningKey { get; set; }

    /// <summary>
    /// Access tokens expire 2 hours after sign in (see .ia/10-backend.md).
    /// </summary>
    public int ExpiryMinutes { get; set; } = 120;
}
