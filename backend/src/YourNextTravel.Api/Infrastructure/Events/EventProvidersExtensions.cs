using YourNextTravel.Api.Infrastructure.Events.Providers;

namespace YourNextTravel.Api.Infrastructure.Events;

public static class EventProvidersExtensions
{
    public static IServiceCollection AddEventProviders(this IServiceCollection services, IConfiguration configuration)
    {
        var footballBaseUrl = configuration["ExternalData:FootballData:BaseUrl"]
            ?? throw new InvalidOperationException("ExternalData:FootballData:BaseUrl is not configured.");
        var openF1BaseUrl = configuration["ExternalData:OpenF1:BaseUrl"]
            ?? throw new InvalidOperationException("ExternalData:OpenF1:BaseUrl is not configured.");
        var ticketmasterBaseUrl = configuration["ExternalData:Ticketmaster:BaseUrl"]
            ?? throw new InvalidOperationException("ExternalData:Ticketmaster:BaseUrl is not configured.");

        // Each provider gets its own AddHttpClient<TImplementation>() registration (a
        // distinct named client keyed by the concrete type) rather than sharing
        // AddHttpClient<IEventProvider, TImplementation>() across all three — the latter
        // would key all three under the same "IEventProvider" client name and the last
        // registration's BaseAddress would silently win for every provider.
        services.AddHttpClient<FootballDataEventProvider>(client =>
        {
            client.BaseAddress = new Uri(footballBaseUrl);
        });
        services.AddTransient<IEventProvider>(sp => sp.GetRequiredService<FootballDataEventProvider>());

        services.AddHttpClient<OpenF1EventProvider>(client =>
        {
            client.BaseAddress = new Uri(openF1BaseUrl);
        });
        services.AddTransient<IEventProvider>(sp => sp.GetRequiredService<OpenF1EventProvider>());

        services.AddHttpClient<TicketmasterEventProvider>(client =>
        {
            client.BaseAddress = new Uri(ticketmasterBaseUrl);
        });
        services.AddTransient<IEventProvider>(sp => sp.GetRequiredService<TicketmasterEventProvider>());

        services.AddScoped<EventAggregatorService>();
        services.AddScoped<IEventMatchingService, EventMatchingService>();

        return services;
    }
}
