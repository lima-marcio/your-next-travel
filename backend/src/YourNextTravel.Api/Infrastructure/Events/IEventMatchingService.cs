using YourNextTravel.Api.Domain.Events;

namespace YourNextTravel.Api.Infrastructure.Events;

public interface IEventMatchingService
{
    /// <summary>
    /// Events matching any of the given interest categories, near the destination
    /// city (within a configurable radius) and around the trip dates (within a
    /// configurable day window either side) — powers Fluxo A's "events near your
    /// destination/dates" section.
    /// </summary>
    Task<IReadOnlyList<NearbyEventMatch>> GetNearDestinationAsync(
        Guid cityId,
        DateOnly startDate,
        DateOnly endDate,
        IReadOnlyCollection<InterestCategory> categories,
        CancellationToken cancellationToken);

    /// <summary>
    /// A capped candidate pool for one interest/horizon — deliberately more than one
    /// result so Fluxo B's feed and "Rolê Aleatório" can vary between calls instead of
    /// always returning the single nearest match.
    /// </summary>
    Task<IReadOnlyList<EventListing>> GetCandidatesAsync(
        InterestCategory category, TimeHorizon horizon, int take, CancellationToken cancellationToken);
}

public sealed record NearbyEventMatch(EventListing Event, double? DistanceKm);
