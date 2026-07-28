using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Events;
using YourNextTravel.Api.Infrastructure.Events;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Features.Discovery;

public class DiscoveryService : IDiscoveryService
{
    private const int FeedCandidatePoolSize = 10;
    private const int FeedSuggestionsPerGroup = 3;
    private const int RandomOutingCandidatePoolSize = 20;

    private static readonly TimeHorizon[] AllHorizons =
    [
        TimeHorizon.Within1Week, TimeHorizon.NextMonth, TimeHorizon.NextSemester
    ];

    private readonly AppDbContext _dbContext;
    private readonly IEventMatchingService _eventMatchingService;
    private readonly IRandomProvider _randomProvider;

    public DiscoveryService(AppDbContext dbContext, IEventMatchingService eventMatchingService, IRandomProvider randomProvider)
    {
        _dbContext = dbContext;
        _eventMatchingService = eventMatchingService;
        _randomProvider = randomProvider;
    }

    public async Task<DiscoveryFeedResponse> GetFeedAsync(Guid userId, CancellationToken cancellationToken)
    {
        var interests = await UserInterestCategoriesAsync(userId, cancellationToken);
        if (interests.Count == 0)
        {
            return new DiscoveryFeedResponse([]);
        }

        var groups = new List<DiscoveryFeedGroup>();

        foreach (var horizon in AllHorizons)
        {
            var suggestions = new List<DiscoverySuggestion>();

            foreach (var category in interests)
            {
                var candidates = await _eventMatchingService.GetCandidatesAsync(
                    category, horizon, FeedCandidatePoolSize, cancellationToken);

                if (candidates.Count == 0)
                {
                    continue;
                }

                var picked = PickRandom(candidates, Math.Min(FeedSuggestionsPerGroup, candidates.Count));
                foreach (var candidate in picked)
                {
                    suggestions.Add(await ToSuggestionAsync(candidate, cancellationToken));
                }
            }

            groups.Add(new DiscoveryFeedGroup(horizon, suggestions));
        }

        return new DiscoveryFeedResponse(groups);
    }

    public async Task<RandomOutingResponse> GetRandomOutingAsync(Guid userId, TimeHorizon? horizon, CancellationToken cancellationToken)
    {
        var interests = await UserInterestCategoriesAsync(userId, cancellationToken);
        if (interests.Count == 0)
        {
            return new RandomOutingResponse(null, "Cadastre ao menos um interesse para receber sugestões de rolê.");
        }

        var horizons = horizon.HasValue ? [horizon.Value] : AllHorizons;

        var pool = new List<EventListing>();
        foreach (var h in horizons)
        {
            foreach (var category in interests)
            {
                pool.AddRange(await _eventMatchingService.GetCandidatesAsync(category, h, RandomOutingCandidatePoolSize, cancellationToken));
            }
        }

        var distinctPool = pool.DistinctBy(e => e.Id).ToList();
        if (distinctPool.Count == 0)
        {
            return new RandomOutingResponse(null, "Nenhum evento encontrado para seus interesses no momento.");
        }

        var picked = distinctPool[_randomProvider.Next(distinctPool.Count)];
        var suggestion = await ToSuggestionAsync(picked, cancellationToken);
        var message = suggestion.CityName is null
            ? $"Que tal: {suggestion.Title}?"
            : $"Que tal: {suggestion.Title} em {suggestion.CityName}?";

        return new RandomOutingResponse(suggestion, message);
    }

    private async Task<List<InterestCategory>> UserInterestCategoriesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Interests
            .Where(i => i.UserId == userId)
            .Select(i => i.Category)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<DiscoverySuggestion> ToSuggestionAsync(EventListing eventListing, CancellationToken cancellationToken)
    {
        string? cityName = null;
        if (eventListing.CityId is not null)
        {
            cityName = await _dbContext.Cities
                .Where(c => c.Id == eventListing.CityId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new DiscoverySuggestion(
            eventListing.Id, eventListing.Category, eventListing.Title, cityName, eventListing.VenueName,
            eventListing.StartUtc, eventListing.EndUtc, eventListing.ExternalUrl);
    }

    private List<EventListing> PickRandom(IReadOnlyList<EventListing> source, int count)
    {
        var pool = new List<EventListing>(source);
        var result = new List<EventListing>();

        for (var i = 0; i < count && pool.Count > 0; i++)
        {
            var index = _randomProvider.Next(pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}
