namespace YourNextTravel.Api.Domain.Users;

public sealed class TravelerProfile
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public ProfileType ProfileType { get; private set; }

    private TravelerProfile()
    {
    }

    public static TravelerProfile CreateDefault(Guid userId)
    {
        return new TravelerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProfileType = ProfileType.Tourist
        };
    }

    public void UpdateProfileType(ProfileType profileType)
    {
        ProfileType = profileType;
    }
}
