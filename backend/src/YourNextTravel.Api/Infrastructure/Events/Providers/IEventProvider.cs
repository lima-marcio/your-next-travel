using YourNextTravel.Api.Domain.Events;

namespace YourNextTravel.Api.Infrastructure.Events.Providers;

public interface IEventProvider
{
    string ProviderName { get; }

    Task<IReadOnlyList<ExternalEvent>> FetchUpcomingAsync(CancellationToken cancellationToken);
}

public sealed record ExternalEvent(
    InterestCategory Category,
    string Title,
    string? Description,
    string? CityName,
    string? VenueName,
    DateTime StartUtc,
    DateTime? EndUtc,
    string ExternalId,
    string? ExternalUrl);
