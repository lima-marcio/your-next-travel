using YourNextTravel.Api.Domain.Events;

namespace YourNextTravel.Api.Features.Discovery;

public interface IDiscoveryService
{
    Task<DiscoveryFeedResponse> GetFeedAsync(Guid userId, CancellationToken cancellationToken);
    Task<RandomOutingResponse> GetRandomOutingAsync(Guid userId, TimeHorizon? horizon, CancellationToken cancellationToken);
}
