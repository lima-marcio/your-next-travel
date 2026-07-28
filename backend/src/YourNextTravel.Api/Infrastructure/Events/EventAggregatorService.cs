using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Events;
using YourNextTravel.Api.Infrastructure.Destinations;
using YourNextTravel.Api.Infrastructure.Events.Providers;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Infrastructure.Events;

/// <summary>
/// Aggregates events from every registered <see cref="IEventProvider"/> and upserts them
/// into EventListing keyed by (ProviderName, ExternalId). Curated events are entered
/// directly into EventListing via Features/Admin and are not fetched through here.
/// </summary>
public class EventAggregatorService
{
    private readonly IEnumerable<IEventProvider> _providers;
    private readonly IDestinationResolver _destinationResolver;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<EventAggregatorService> _logger;

    public EventAggregatorService(
        IEnumerable<IEventProvider> providers,
        IDestinationResolver destinationResolver,
        AppDbContext dbContext,
        ILogger<EventAggregatorService> logger)
    {
        _providers = providers;
        _destinationResolver = destinationResolver;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RefreshAllAsync(CancellationToken cancellationToken)
    {
        foreach (var provider in _providers)
        {
            IReadOnlyList<ExternalEvent> events;
            try
            {
                events = await provider.FetchUpcomingAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Event provider {ProviderName} failed.", provider.ProviderName);
                continue;
            }

            foreach (var externalEvent in events)
            {
                await UpsertAsync(provider.ProviderName, externalEvent, cancellationToken);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertAsync(string providerName, ExternalEvent externalEvent, CancellationToken cancellationToken)
    {
        Guid? cityId = null;
        if (!string.IsNullOrWhiteSpace(externalEvent.CityName))
        {
            var city = await _destinationResolver.ResolveAsync(externalEvent.CityName, cancellationToken);
            cityId = city?.Id;
        }

        var existing = await _dbContext.EventListings.FirstOrDefaultAsync(
            e => e.ProviderName == providerName && e.ExternalId == externalEvent.ExternalId, cancellationToken);

        if (existing is not null)
        {
            existing.RefreshFromSource(
                externalEvent.Title, externalEvent.Description, cityId, externalEvent.VenueName,
                externalEvent.StartUtc, externalEvent.EndUtc, externalEvent.ExternalUrl);
            return;
        }

        _dbContext.EventListings.Add(EventListing.Create(
            externalEvent.Category, externalEvent.Title, externalEvent.Description, cityId,
            externalEvent.VenueName, externalEvent.StartUtc, externalEvent.EndUtc,
            providerName, externalEvent.ExternalId, externalEvent.ExternalUrl));
    }
}
