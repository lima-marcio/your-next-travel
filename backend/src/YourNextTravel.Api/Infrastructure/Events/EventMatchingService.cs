using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Events;
using YourNextTravel.Api.Infrastructure.Destinations;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Infrastructure.Events;

public class EventMatchingService : IEventMatchingService
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public EventMatchingService(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<NearbyEventMatch>> GetNearDestinationAsync(
        Guid cityId,
        DateOnly startDate,
        DateOnly endDate,
        IReadOnlyCollection<InterestCategory> categories,
        CancellationToken cancellationToken)
    {
        if (categories.Count == 0)
        {
            return [];
        }

        var destinationCity = await _dbContext.Cities.FirstOrDefaultAsync(c => c.Id == cityId, cancellationToken);
        if (destinationCity is null)
        {
            return [];
        }

        var radiusKm = _configuration.GetValue("DestinationGuide:EventProximityKm", 500);
        var windowDays = _configuration.GetValue("DestinationGuide:EventDateWindowDays", 3);

        var windowStart = startDate.AddDays(-windowDays).ToDateTime(TimeOnly.MinValue);
        var windowEnd = endDate.AddDays(windowDays).ToDateTime(TimeOnly.MaxValue);

        var candidates = await _dbContext.EventListings
            .Where(e => categories.Contains(e.Category) && e.StartUtc >= windowStart && e.StartUtc <= windowEnd && e.CityId != null)
            .ToListAsync(cancellationToken);

        var matches = new List<NearbyEventMatch>();
        foreach (var candidate in candidates)
        {
            var eventCity = await _dbContext.Cities.FirstOrDefaultAsync(c => c.Id == candidate.CityId, cancellationToken);
            if (eventCity is null)
            {
                continue;
            }

            var distanceKm = GeoDistance.KilometersBetween(
                destinationCity.Latitude, destinationCity.Longitude, eventCity.Latitude, eventCity.Longitude);

            if (distanceKm <= radiusKm)
            {
                matches.Add(new NearbyEventMatch(candidate, distanceKm));
            }
        }

        return matches.OrderBy(m => m.DistanceKm).ThenBy(m => m.Event.StartUtc).ToList();
    }

    public async Task<IReadOnlyList<EventListing>> GetCandidatesAsync(
        InterestCategory category, TimeHorizon horizon, int take, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var horizonEnd = horizon switch
        {
            TimeHorizon.Within1Week => now.AddDays(7),
            TimeHorizon.NextMonth => now.AddMonths(1),
            TimeHorizon.NextSemester => now.AddMonths(6),
            _ => throw new ArgumentOutOfRangeException(nameof(horizon))
        };

        return await _dbContext.EventListings
            .Where(e => e.Category == category && e.StartUtc >= now && e.StartUtc <= horizonEnd)
            .OrderBy(e => e.StartUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
