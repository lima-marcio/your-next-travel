using System.ComponentModel.DataAnnotations;

namespace YourNextTravel.Api.Features.Auth;

public class RegisterRequest
{
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public required string Email { get; set; }

    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "Password must include an uppercase letter, a lowercase letter, a digit, and a special character.")]
    public required string Password { get; set; }

    public required string DisplayName { get; set; }
}

public class LoginRequest
{
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public required string Email { get; set; }

    public required string Password { get; set; }
}

public class GoogleLoginRequest
{
    public required string IdToken { get; set; }
}

public record AuthResponse(string Token, DateTime ExpiresAtUtc, string Email, string DisplayName, string Role);

public record CurrentUserResponse(Guid Id, string Email, string DisplayName, string Role);
