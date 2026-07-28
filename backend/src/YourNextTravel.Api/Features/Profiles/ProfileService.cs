using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Features.Profiles;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _dbContext;

    public ProfileService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TravelerProfileResponse> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await FindProfileAsync(userId, cancellationToken);
        return new TravelerProfileResponse(profile.ProfileType);
    }

    public async Task<TravelerProfileResponse> UpdateAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await FindProfileAsync(userId, cancellationToken);
        profile.UpdateProfileType(request.ProfileType);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new TravelerProfileResponse(profile.ProfileType);
    }

    private async Task<Domain.Users.TravelerProfile> FindProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.TravelerProfiles.SingleAsync(p => p.UserId == userId, cancellationToken);
    }
}
