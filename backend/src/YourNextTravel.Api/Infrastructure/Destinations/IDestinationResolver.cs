using YourNextTravel.Api.Domain.Destinations;

namespace YourNextTravel.Api.Infrastructure.Destinations;

public interface IDestinationResolver
{
    /// <summary>
    /// Resolves free-text like "Rome" to a City, creating the Country/City rows on
    /// first lookup if they are not already cached locally. Returns null if the query
    /// does not match any place.
    /// </summary>
    Task<City?> ResolveAsync(string freeTextQuery, CancellationToken cancellationToken);
}
