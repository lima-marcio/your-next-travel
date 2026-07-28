using YourNextTravel.Api.Domain.Users;

namespace YourNextTravel.Api.Features.Profiles;

public record TravelerProfileResponse(ProfileType ProfileType);

public class UpdateProfileRequest
{
    public required ProfileType ProfileType { get; set; }
}
