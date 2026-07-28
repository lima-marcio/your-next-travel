using YourNextTravel.Api.Domain.Events;
using YourNextTravel.Api.Domain.Users;

namespace YourNextTravel.Api.Features.Dossier;

public class DossierSearchRequest
{
    public required string Destination { get; set; }
    public required DateOnly StartDate { get; set; }
    public required DateOnly EndDate { get; set; }
    public ProfileType? ProfileTypeOverride { get; set; }
}

public record WeatherSummaryResponse(bool Available, bool IsForecast, double? AvgTempC, double? MinTempC, double? MaxTempC, double? PrecipitationMm);

public record CurrencySummaryResponse(bool Available, string HomeCurrencyCode, string LocalCurrencyCode, decimal? HomeToLocalRate, DateOnly? AsOfDate);

public record LodgingSummaryResponse(
    bool Available, decimal? AvgNightlyAmount, string? Currency, decimal? MinNightlyAmount, decimal? MaxNightlyAmount,
    DateOnly? SampleWindowStart, DateOnly? SampleWindowEnd);

public record LegalHealthSummaryResponse(
    bool Available, string? VisaRequirementText, string? VaccinationRequirementText, string? OtherHealthNotes, string? SourceNote);

public record BudgetSummaryResponse(decimal LodgingComponentAmount, decimal MiscDailyComponentAmount, decimal TotalAmount, string Currency, string AssumptionsNote);

public record MatchingEventResponse(
    Guid Id, InterestCategory Category, string Title, string? VenueName, DateTime StartUtc, DateTime? EndUtc, double? DistanceKm, string? ExternalUrl);

public record DossierResponse(
    Guid SearchId,
    string CityName,
    string CountryName,
    DateOnly StartDate,
    DateOnly EndDate,
    ProfileType ProfileTypeUsed,
    WeatherSummaryResponse Weather,
    CurrencySummaryResponse Currency,
    LodgingSummaryResponse Lodging,
    LegalHealthSummaryResponse LegalHealth,
    BudgetSummaryResponse Budget,
    IReadOnlyList<MatchingEventResponse> MatchingEvents);

public record DestinationSearchSummaryResponse(Guid SearchId, string CityName, DateOnly StartDate, DateOnly EndDate, DateTime CreatedAtUtc);
