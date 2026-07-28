namespace YourNextTravel.Api.Features.Interests;

public interface IInterestService
{
    Task<IReadOnlyList<InterestResponse>> ListAsync(Guid userId, CancellationToken cancellationToken);
    Task<InterestResponse> AddAsync(Guid userId, CreateInterestRequest request, CancellationToken cancellationToken);
    Task RemoveAsync(Guid userId, Guid interestId, CancellationToken cancellationToken);
}
