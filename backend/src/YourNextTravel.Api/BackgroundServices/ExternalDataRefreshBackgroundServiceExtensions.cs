namespace YourNextTravel.Api.BackgroundServices;

public static class ExternalDataRefreshBackgroundServiceExtensions
{
    public static IServiceCollection AddExternalDataRefreshBackgroundService(this IServiceCollection services)
    {
        services.AddHostedService<ExternalDataRefreshBackgroundService>();
        return services;
    }
}
