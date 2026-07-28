using YourNextTravel.Api.Domain.Events;

namespace YourNextTravel.Api.Features.Discovery;

public record DiscoverySuggestion(
    Guid EventId, InterestCategory Category, string Title, string? CityName, string? VenueName,
    DateTime StartUtc, DateTime? EndUtc, string? ExternalUrl);

public record DiscoveryFeedGroup(TimeHorizon Horizon, IReadOnlyList<DiscoverySuggestion> Suggestions);

public record DiscoveryFeedResponse(IReadOnlyList<DiscoveryFeedGroup> Groups);

public record RandomOutingResponse(DiscoverySuggestion? Suggestion, string Message);
