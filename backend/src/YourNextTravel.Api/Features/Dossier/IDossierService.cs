namespace YourNextTravel.Api.Features.Dossier;

public interface IDossierService
{
    Task<DossierResponse> SearchAsync(Guid userId, DossierSearchRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<DestinationSearchSummaryResponse>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken);
    Task<DossierResponse> GetBySearchIdAsync(Guid userId, Guid searchId, CancellationToken cancellationToken);
}
