using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Users;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Features.Auth;

public class AuthService : IAuthService
{
    // Fixed, valid-format PasswordHasher<User> v3 hash used solely to burn equivalent CPU time
    // when an email lookup fails or the account has no password (Google-only), so that these
    // cases are not distinguishable from "wrong password" by response timing.
    private const string DummyPasswordHash =
        "AQAAAAIAAYagAAAAEBag4MgoHkTKwpWe2+sKxK60skErbY5tpCPT1jjrfod0ASiHb5X9WA7anEBRxBTQAA==";

    private readonly AppDbContext _dbContext;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext dbContext,
        IJwtTokenGenerator tokenGenerator,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _tokenGenerator = tokenGenerator;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();
        var emailTaken = await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (emailTaken)
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var passwordHash = _passwordHasher.HashPassword(null!, request.Password);
        var user = User.CreateWithPassword(request.Email, passwordHash, request.DisplayName);

        _dbContext.Users.Add(user);
        _dbContext.TravelerProfiles.Add(TravelerProfile.CreateDefault(user.Id));
        await _dbContext.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user is null || user.PasswordHash is null)
        {
            // Pay the same PBKDF2 verification cost as the "user found, has password" path so
            // that response latency does not leak whether the email exists or is Google-only
            // (timing side channel / account enumeration).
            _passwordHasher.VerifyHashedPassword(null!, DummyPasswordHash, request.Password);
            return null;
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponse> LoginWithGoogleAsync(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var clientId = _configuration["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId is not configured.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            });
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedAccessException("Invalid Google ID token.");
        }

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.GoogleSubjectId == payload.Subject, cancellationToken);

        if (user is null)
        {
            var normalizedEmail = payload.Email.ToLowerInvariant();
            user = await _dbContext.Users
                .SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

            if (user is not null)
            {
                user.LinkGoogleAccount(payload.Subject);
            }
            else
            {
                user = User.CreateWithGoogle(payload.Email, payload.Subject, payload.Name ?? payload.Email);
                _dbContext.Users.Add(user);
                _dbContext.TravelerProfiles.Add(TravelerProfile.CreateDefault(user.Id));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return BuildAuthResponse(user);
    }

    public CurrentUserResponse GetCurrentUser(ClaimsPrincipal principal)
    {
        var id = Guid.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var email = principal.FindFirstValue(ClaimTypes.Email)!;
        var displayName = principal.FindFirstValue(ClaimTypes.Name)!;
        var role = principal.FindFirstValue(ClaimTypes.Role)!;

        return new CurrentUserResponse(id, email, displayName, role);
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var (token, expiresAtUtc) = _tokenGenerator.GenerateToken(user);
        return new AuthResponse(token, expiresAtUtc, user.Email, user.DisplayName, user.Role.ToString());
    }
}
