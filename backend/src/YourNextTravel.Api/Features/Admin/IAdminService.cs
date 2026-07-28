namespace YourNextTravel.Api.Features.Admin;

public interface IAdminService
{
    Task<IReadOnlyList<LegalHealthRequirementResponse>> ListLegalHealthRequirementsAsync(CancellationToken cancellationToken);
    Task<LegalHealthRequirementResponse> UpsertLegalHealthRequirementAsync(UpsertLegalHealthRequirementRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<CuratedEventResponse>> ListCuratedEventsAsync(CancellationToken cancellationToken);
    Task<CuratedEventResponse> CreateCuratedEventAsync(CreateCuratedEventRequest request, CancellationToken cancellationToken);
    Task DeleteCuratedEventAsync(Guid eventId, CancellationToken cancellationToken);
}
