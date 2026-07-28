namespace YourNextTravel.Api.Infrastructure.Weather;

public static class WeatherProviderExtensions
{
    public static IServiceCollection AddWeatherProvider(this IServiceCollection services)
    {
        services.AddHttpClient<IWeatherProvider, OpenMeteoWeatherProvider>();
        return services;
    }
}
