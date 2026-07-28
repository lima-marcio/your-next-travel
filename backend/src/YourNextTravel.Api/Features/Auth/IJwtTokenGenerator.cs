using YourNextTravel.Api.Domain.Users;

namespace YourNextTravel.Api.Features.Auth;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
