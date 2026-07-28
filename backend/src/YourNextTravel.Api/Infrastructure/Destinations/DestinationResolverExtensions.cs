namespace YourNextTravel.Api.Infrastructure.Destinations;

public static class DestinationResolverExtensions
{
    public static IServiceCollection AddDestinationResolver(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["ExternalData:OpenMeteo:GeocodingBaseUrl"]
            ?? throw new InvalidOperationException("ExternalData:OpenMeteo:GeocodingBaseUrl is not configured.");

        services.AddHttpClient<IDestinationResolver, OpenMeteoGeocodingResolver>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }
}
