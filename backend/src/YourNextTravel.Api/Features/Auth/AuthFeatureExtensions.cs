using Microsoft.AspNetCore.Identity;
using YourNextTravel.Api.Domain.Users;

namespace YourNextTravel.Api.Features.Auth;

public static class AuthFeatureExtensions
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
