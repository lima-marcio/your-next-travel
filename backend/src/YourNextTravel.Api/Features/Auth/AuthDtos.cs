namespace YourNextTravel.Api.Features.Auth;

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string DisplayName { get; set; }
}

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class GoogleLoginRequest
{
    public required string IdToken { get; set; }
}

public record AuthResponse(string Token, DateTime ExpiresAtUtc, string Email, string DisplayName, string Role);

public record CurrentUserResponse(Guid Id, string Email, string DisplayName, string Role);
