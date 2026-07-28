namespace YourNextTravel.Api.Infrastructure.Lodging;

public static class LodgingProviderExtensions
{
    public static IServiceCollection AddLodgingProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["ExternalData:Amadeus:BaseUrl"]
            ?? throw new InvalidOperationException("ExternalData:Amadeus:BaseUrl is not configured.");

        services.AddHttpClient<ILodgingPriceProvider, AmadeusLodgingPriceProvider>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }
}
