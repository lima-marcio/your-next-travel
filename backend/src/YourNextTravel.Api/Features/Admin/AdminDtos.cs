using YourNextTravel.Api.Domain.Events;

namespace YourNextTravel.Api.Features.Admin;

public record LegalHealthRequirementResponse(
    Guid Id,
    string CountryIsoCode2,
    string VisaRequirementText,
    string VaccinationRequirementText,
    string? OtherHealthNotes,
    string SourceNote,
    DateTime LastReviewedAtUtc);

public class UpsertLegalHealthRequirementRequest
{
    public required string CountryIsoCode2 { get; set; }
    public required string CountryName { get; set; }
    public required string CurrencyCode { get; set; }
    public required string VisaRequirementText { get; set; }
    public required string VaccinationRequirementText { get; set; }
    public string? OtherHealthNotes { get; set; }
    public required string SourceNote { get; set; }
}

public record CuratedEventResponse(
    Guid Id,
    InterestCategory Category,
    string Title,
    string? Description,
    string? VenueName,
    DateTime StartUtc,
    DateTime? EndUtc,
    string? ExternalUrl);

public class CreateCuratedEventRequest
{
    public required InterestCategory Category { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? CityName { get; set; }
    public string? VenueName { get; set; }
    public required DateTime StartUtc { get; set; }
    public DateTime? EndUtc { get; set; }
    public string? ExternalUrl { get; set; }
}
