namespace YourNextTravel.Api.Features.DestinationGuide;

public interface IDestinationGuideService
{
    Task<DestinationGuideResponse> SearchAsync(Guid userId, DestinationGuideSearchRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<DestinationSearchSummaryResponse>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken);
    Task<DestinationGuideResponse> GetBySearchIdAsync(Guid userId, Guid searchId, CancellationToken cancellationToken);
}
