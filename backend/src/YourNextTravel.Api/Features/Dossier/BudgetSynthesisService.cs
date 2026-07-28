using System.Text;
using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Pricing;
using YourNextTravel.Api.Domain.Users;
using YourNextTravel.Api.Infrastructure.Currency;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Features.Dossier;

public class BudgetSynthesisService : IBudgetSynthesisService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly IConfiguration _configuration;

    public BudgetSynthesisService(AppDbContext dbContext, ICurrencyConverter currencyConverter, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _currencyConverter = currencyConverter;
        _configuration = configuration;
    }

    public async Task<BudgetCalculation> CalculateAsync(
        Guid countryId,
        string localCurrencyCode,
        int nights,
        ProfileType profileType,
        LodgingPriceEstimate? lodgingEstimate,
        CancellationToken cancellationToken)
    {
        localCurrencyCode = localCurrencyCode.ToUpperInvariant();
        var notes = new StringBuilder();

        var profileMultiplier = _configuration.GetValue($"Budget:ProfileMultipliers:{profileType}", 1.0m);
        notes.Append($"Perfil {profileType} aplica multiplicador {profileMultiplier}. ");

        var countryCostIndex = await _dbContext.CountryCostIndexes
            .Where(c => c.CountryId == countryId)
            .Select(c => (decimal?)c.Multiplier)
            .FirstOrDefaultAsync(cancellationToken) ?? 1.0m;

        if (countryCostIndex == 1.0m)
        {
            notes.Append("Sem índice de custo de vida curado para este país; usando multiplicador padrão 1.0. ");
        }

        var baselineAmount = _configuration.GetValue("Budget:BaselineDailyMiscSpend:Amount", 40m);
        var baselineCurrency = _configuration["Budget:BaselineDailyMiscSpend:Currency"] ?? "USD";

        var convertedBaseline = await _currencyConverter.ConvertAsync(baselineAmount, baselineCurrency, localCurrencyCode, cancellationToken);
        var miscDailyAmount = convertedBaseline ?? baselineAmount;
        if (convertedBaseline is null)
        {
            notes.Append($"Câmbio {baselineCurrency}->{localCurrencyCode} indisponível; valor diário aproximado sem conversão. ");
        }

        var miscDailyComponentAmount = miscDailyAmount * countryCostIndex * profileMultiplier * nights;

        decimal lodgingComponentAmount;
        if (lodgingEstimate is null)
        {
            lodgingComponentAmount = 0;
            notes.Append("Sem estimativa de hospedagem em cache para este destino ainda. ");
        }
        else
        {
            var convertedNightly = await _currencyConverter.ConvertAsync(
                lodgingEstimate.AvgNightlyAmount, lodgingEstimate.Currency, localCurrencyCode, cancellationToken);
            var nightlyAmount = convertedNightly ?? lodgingEstimate.AvgNightlyAmount;
            if (convertedNightly is null && lodgingEstimate.Currency != localCurrencyCode)
            {
                notes.Append($"Câmbio {lodgingEstimate.Currency}->{localCurrencyCode} indisponível; valor de hospedagem sem conversão. ");
            }

            lodgingComponentAmount = nightlyAmount * profileMultiplier * nights;
            notes.Append("Preço de hospedagem vem do ambiente sandbox da Amadeus, não reflete disponibilidade/preço reais de produção. ");
        }

        return new BudgetCalculation(
            lodgingComponentAmount, miscDailyComponentAmount, lodgingComponentAmount + miscDailyComponentAmount,
            localCurrencyCode, notes.ToString().Trim());
    }
}
