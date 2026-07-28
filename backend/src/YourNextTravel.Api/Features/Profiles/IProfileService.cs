namespace YourNextTravel.Api.Features.Profiles;

public interface IProfileService
{
    Task<TravelerProfileResponse> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<TravelerProfileResponse> UpdateAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken);
}
