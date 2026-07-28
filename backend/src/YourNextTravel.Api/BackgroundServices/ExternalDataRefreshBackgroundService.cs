using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Infrastructure.Currency;
using YourNextTravel.Api.Infrastructure.Events;
using YourNextTravel.Api.Infrastructure.Lodging;
using YourNextTravel.Api.Infrastructure.Persistence;
using YourNextTravel.Api.Infrastructure.Weather;

namespace YourNextTravel.Api.BackgroundServices;

/// <summary>
/// A single periodic job refreshes every external-data cache (events, currency,
/// weather, lodging) rather than one BackgroundService per source, with per-source
/// staleness checks inside — see ExternalData:Staleness:* in configuration.
/// </summary>
public sealed class ExternalDataRefreshBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ExternalDataRefreshBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = configuration.GetValue("ExternalData:BackgroundService:IntervalSeconds", 3600);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

        do
        {
            await RefreshAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await RefreshEventsAsync(scope, cancellationToken);
        await RefreshCurrencyRatesAsync(scope, dbContext, cancellationToken);
        await RefreshCityDataAsync(scope, dbContext, cancellationToken);
    }

    private async Task RefreshEventsAsync(IServiceScope scope, CancellationToken cancellationToken)
    {
        try
        {
            var eventAggregator = scope.ServiceProvider.GetRequiredService<EventAggregatorService>();
            await eventAggregator.RefreshAllAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Event refresh failed.");
        }
    }

    private async Task RefreshCurrencyRatesAsync(IServiceScope scope, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            var currencyCodes = await dbContext.Countries
                .Select(c => c.CurrencyCode)
                .Distinct()
                .ToListAsync(cancellationToken);
            currencyCodes.Add("USD");

            var currencyProvider = scope.ServiceProvider.GetRequiredService<ICurrencyProvider>();
            await currencyProvider.RefreshRatesAsync(currencyCodes.Distinct().ToList(), cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Currency refresh failed.");
        }
    }

    private async Task RefreshCityDataAsync(IServiceScope scope, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var staleWeatherHours = configuration.GetValue("ExternalData:Staleness:WeatherHours", 6);
        var staleLodgingHours = configuration.GetValue("ExternalData:Staleness:LodgingHours", 12);
        var now = DateTime.UtcNow;

        var cities = await dbContext.Cities.ToListAsync(cancellationToken);

        var weatherProvider = scope.ServiceProvider.GetRequiredService<IWeatherProvider>();
        var lodgingProvider = scope.ServiceProvider.GetRequiredService<ILodgingPriceProvider>();

        foreach (var city in cities)
        {
            var latestWeatherFetch = await dbContext.WeatherSnapshots
                .Where(w => w.CityId == city.Id)
                .OrderByDescending(w => w.FetchedAtUtc)
                .Select(w => (DateTime?)w.FetchedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestWeatherFetch is null || (now - latestWeatherFetch.Value).TotalHours > staleWeatherHours)
            {
                try
                {
                    await weatherProvider.RefreshClimatologyAsync(city, cancellationToken);
                    await weatherProvider.RefreshForecastAsync(city, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Weather refresh failed for city {CityId}.", city.Id);
                }
            }

            var latestLodgingFetch = await dbContext.LodgingPriceEstimates
                .Where(l => l.CityId == city.Id)
                .OrderByDescending(l => l.FetchedAtUtc)
                .Select(l => (DateTime?)l.FetchedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestLodgingFetch is null || (now - latestLodgingFetch.Value).TotalHours > staleLodgingHours)
            {
                try
                {
                    await lodgingProvider.RefreshPriceEstimateAsync(city, cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogWarning(ex, "Lodging refresh failed for city {CityId}.", city.Id);
                }
            }
        }
    }
}
