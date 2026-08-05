namespace YourNextTravel.Api.Features.DestinationGuide;

public static class DestinationGuideFeatureExtensions
{
    public static IServiceCollection AddDestinationGuideFeature(this IServiceCollection services)
    {
        services.AddScoped<IBudgetSynthesisService, BudgetSynthesisService>();
        services.AddScoped<IDestinationGuideService, DestinationGuideService>();
        return services;
    }
}
