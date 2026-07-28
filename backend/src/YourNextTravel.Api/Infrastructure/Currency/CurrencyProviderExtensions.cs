namespace YourNextTravel.Api.Infrastructure.Currency;

public static class CurrencyProviderExtensions
{
    public static IServiceCollection AddCurrencyProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["ExternalData:Frankfurter:BaseUrl"]
            ?? throw new InvalidOperationException("ExternalData:Frankfurter:BaseUrl is not configured.");

        services.AddHttpClient<ICurrencyProvider, FrankfurterCurrencyProvider>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddScoped<ICurrencyConverter, CurrencyConverter>();

        return services;
    }
}
