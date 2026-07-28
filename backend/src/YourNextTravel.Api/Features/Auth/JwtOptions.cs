namespace YourNextTravel.Api.Features.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string SigningKey { get; set; }

    /// <summary>
    /// No refresh-token handling for MVP (matching the workspace's existing
    /// siblings' simplicity). Expiry is longer than a typical 60-minute default
    /// because this is a low-stakes informational app with no sensitive
    /// transaction to protect with a short-lived token.
    /// </summary>
    public int ExpiryMinutes { get; set; } = 1440;
}
