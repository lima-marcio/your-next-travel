namespace YourNextTravel.Api.Features.Discovery;

public static class DiscoveryFeatureExtensions
{
    public static IServiceCollection AddDiscoveryFeature(this IServiceCollection services)
    {
        services.AddSingleton<IRandomProvider, RandomProvider>();
        services.AddScoped<IDiscoveryService, DiscoveryService>();
        return services;
    }
}
