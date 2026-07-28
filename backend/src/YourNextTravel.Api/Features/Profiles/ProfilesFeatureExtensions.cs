namespace YourNextTravel.Api.Features.Profiles;

public static class ProfilesFeatureExtensions
{
    public static IServiceCollection AddProfilesFeature(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, ProfileService>();
        return services;
    }
}
