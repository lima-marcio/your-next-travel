using YourNextTravel.Api.Domain.Pricing;
using YourNextTravel.Api.Domain.Users;

namespace YourNextTravel.Api.Features.Dossier;

public interface IBudgetSynthesisService
{
    Task<BudgetCalculation> CalculateAsync(
        Guid countryId,
        string localCurrencyCode,
        int nights,
        ProfileType profileType,
        LodgingPriceEstimate? lodgingEstimate,
        CancellationToken cancellationToken);
}

public sealed record BudgetCalculation(
    decimal LodgingComponentAmount, decimal MiscDailyComponentAmount, decimal TotalAmount, string Currency, string AssumptionsNote);
