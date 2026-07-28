namespace YourNextTravel.Api.Features.Interests;

public static class InterestsFeatureExtensions
{
    public static IServiceCollection AddInterestsFeature(this IServiceCollection services)
    {
        services.AddScoped<IInterestService, InterestService>();
        return services;
    }
}
