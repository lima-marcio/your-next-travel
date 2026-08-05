using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Budget;
using YourNextTravel.Api.Domain.Destinations;
using YourNextTravel.Api.Domain.Pricing;
using YourNextTravel.Api.Domain.Weather;
using YourNextTravel.Api.Infrastructure.Currency;
using YourNextTravel.Api.Infrastructure.Destinations;
using YourNextTravel.Api.Infrastructure.Events;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Features.DestinationGuide;

public class DestinationGuideService : IDestinationGuideService
{
    private readonly AppDbContext _dbContext;
    private readonly IDestinationResolver _destinationResolver;
    private readonly IEventMatchingService _eventMatchingService;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly IBudgetSynthesisService _budgetSynthesisService;
    private readonly IConfiguration _configuration;

    public DestinationGuideService(
        AppDbContext dbContext,
        IDestinationResolver destinationResolver,
        IEventMatchingService eventMatchingService,
        ICurrencyConverter currencyConverter,
        IBudgetSynthesisService budgetSynthesisService,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _destinationResolver = destinationResolver;
        _eventMatchingService = eventMatchingService;
        _currencyConverter = currencyConverter;
        _budgetSynthesisService = budgetSynthesisService;
        _configuration = configuration;
    }

    public async Task<DestinationGuideResponse> SearchAsync(Guid userId, DestinationGuideSearchRequest request, CancellationToken cancellationToken)
    {
        var city = await _destinationResolver.ResolveAsync(request.Destination, cancellationToken)
            ?? throw new ArgumentException($"Could not find a destination matching '{request.Destination}'.");

        var country = await _dbContext.Countries.FirstAsync(c => c.Id == city.CountryId, cancellationToken);

        var profileType = request.ProfileTypeOverride
            ?? (await _dbContext.TravelerProfiles.FirstAsync(p => p.UserId == userId, cancellationToken)).ProfileType;

        var search = DestinationSearch.Create(userId, city.Id, request.StartDate, request.EndDate, profileType);
        _dbContext.DestinationSearches.Add(search);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var lodgingEstimate = await LatestLodgingEstimateAsync(city.Id, cancellationToken);

        var budget = await _budgetSynthesisService.CalculateAsync(
            country.Id, country.CurrencyCode, search.Nights, profileType, lodgingEstimate, cancellationToken);

        var budgetEstimate = BudgetEstimate.Create(
            search.Id, budget.LodgingComponentAmount, budget.MiscDailyComponentAmount, budget.Currency, budget.AssumptionsNote);
        _dbContext.BudgetEstimates.Add(budgetEstimate);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var (weather, currency, lodging, legalHealth, events) = await BuildLiveSectionsAsync(
            userId, city, country, request.StartDate, request.EndDate, lodgingEstimate, cancellationToken);

        return new DestinationGuideResponse(
            search.Id, city.Name, country.Name, request.StartDate, request.EndDate, profileType,
            weather, currency, lodging, legalHealth,
            new BudgetSummaryResponse(budget.LodgingComponentAmount, budget.MiscDailyComponentAmount, budget.TotalAmount, budget.Currency, budget.AssumptionsNote),
            events);
    }

    public async Task<IReadOnlyList<DestinationSearchSummaryResponse>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.DestinationSearches
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAtUtc)
            .Join(_dbContext.Cities, s => s.CityId, c => c.Id,
                (s, c) => new DestinationSearchSummaryResponse(s.Id, c.Name, s.StartDate, s.EndDate, s.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<DestinationGuideResponse> GetBySearchIdAsync(Guid userId, Guid searchId, CancellationToken cancellationToken)
    {
        var search = await _dbContext.DestinationSearches
            .FirstOrDefaultAsync(s => s.Id == searchId && s.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Destination search not found.");

        var city = await _dbContext.Cities.FirstAsync(c => c.Id == search.CityId, cancellationToken);
        var country = await _dbContext.Countries.FirstAsync(c => c.Id == city.CountryId, cancellationToken);

        var budgetEstimate = await _dbContext.BudgetEstimates
            .FirstOrDefaultAsync(b => b.DestinationSearchId == search.Id, cancellationToken)
            ?? throw new InvalidOperationException("Budget estimate missing for this search.");

        var lodgingEstimate = await LatestLodgingEstimateAsync(city.Id, cancellationToken);

        var (weather, currency, lodging, legalHealth, events) = await BuildLiveSectionsAsync(
            userId, city, country, search.StartDate, search.EndDate, lodgingEstimate, cancellationToken);

        return new DestinationGuideResponse(
            search.Id, city.Name, country.Name, search.StartDate, search.EndDate, search.TravelerProfileTypeUsed,
            weather, currency, lodging, legalHealth,
            new BudgetSummaryResponse(
                budgetEstimate.LodgingComponentAmount, budgetEstimate.MiscDailyComponentAmount, budgetEstimate.TotalAmount,
                budgetEstimate.Currency, budgetEstimate.AssumptionsNote),
            events);
    }

    private async Task<LodgingPriceEstimate?> LatestLodgingEstimateAsync(Guid cityId, CancellationToken cancellationToken)
    {
        return await _dbContext.LodgingPriceEstimates
            .Where(l => l.CityId == cityId)
            .OrderByDescending(l => l.FetchedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<(
        WeatherSummaryResponse Weather,
        CurrencySummaryResponse Currency,
        LodgingSummaryResponse Lodging,
        LegalHealthSummaryResponse LegalHealth,
        IReadOnlyList<MatchingEventResponse> Events)> BuildLiveSectionsAsync(
        Guid userId, City city, Country country, DateOnly startDate, DateOnly endDate,
        LodgingPriceEstimate? lodgingEstimate, CancellationToken cancellationToken)
    {
        var weather = await BuildWeatherSummaryAsync(city.Id, startDate, endDate, cancellationToken);
        var currency = await BuildCurrencySummaryAsync(country.CurrencyCode, cancellationToken);
        var lodging = lodgingEstimate is null
            ? new LodgingSummaryResponse(false, null, null, null, null, null, null)
            : new LodgingSummaryResponse(
                true, lodgingEstimate.AvgNightlyAmount, lodgingEstimate.Currency, lodgingEstimate.MinNightlyAmount,
                lodgingEstimate.MaxNightlyAmount, lodgingEstimate.SampleWindowStart, lodgingEstimate.SampleWindowEnd);

        var legalHealthRequirement = await _dbContext.LegalHealthRequirements
            .FirstOrDefaultAsync(r => r.CountryId == country.Id, cancellationToken);
        var legalHealth = legalHealthRequirement is null
            ? new LegalHealthSummaryResponse(false, null, null, null, null)
            : new LegalHealthSummaryResponse(
                true, legalHealthRequirement.VisaRequirementText, legalHealthRequirement.VaccinationRequirementText,
                legalHealthRequirement.OtherHealthNotes, legalHealthRequirement.SourceNote);

        var interestCategories = await _dbContext.Interests
            .Where(i => i.UserId == userId)
            .Select(i => i.Category)
            .Distinct()
            .ToListAsync(cancellationToken);

        var matches = await _eventMatchingService.GetNearDestinationAsync(city.Id, startDate, endDate, interestCategories, cancellationToken);
        var events = matches
            .Select(m => new MatchingEventResponse(
                m.Event.Id, m.Event.Category, m.Event.Title, m.Event.VenueName, m.Event.StartUtc, m.Event.EndUtc, m.DistanceKm, m.Event.ExternalUrl))
            .ToList();

        return (weather, currency, lodging, legalHealth, events);
    }

    private async Task<WeatherSummaryResponse> BuildWeatherSummaryAsync(
        Guid cityId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var forecastHorizonEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(16));

        if (startDate <= forecastHorizonEnd)
        {
            var forecasts = await _dbContext.WeatherSnapshots
                .Where(w => w.CityId == cityId && w.Granularity == WeatherGranularity.DailyForecast
                    && w.ForDate >= startDate && w.ForDate <= endDate)
                .ToListAsync(cancellationToken);

            if (forecasts.Count > 0)
            {
                return new WeatherSummaryResponse(
                    true, true, forecasts.Average(f => f.AvgTempC), forecasts.Average(f => f.MinTempC),
                    forecasts.Average(f => f.MaxTempC), forecasts.Average(f => f.PrecipitationMm));
            }
        }

        var months = Enumerable.Range(0, endDate.DayNumber - startDate.DayNumber + 1)
            .Select(offset => startDate.AddDays(offset).Month)
            .Distinct()
            .ToList();

        var climatology = await _dbContext.WeatherSnapshots
            .Where(w => w.CityId == cityId && w.Granularity == WeatherGranularity.MonthlyClimatology && months.Contains(w.Month!.Value))
            .ToListAsync(cancellationToken);

        if (climatology.Count == 0)
        {
            return new WeatherSummaryResponse(false, false, null, null, null, null);
        }

        return new WeatherSummaryResponse(
            true, false, climatology.Average(c => c.AvgTempC), climatology.Average(c => c.MinTempC),
            climatology.Average(c => c.MaxTempC), climatology.Average(c => c.PrecipitationMm));
    }

    private async Task<CurrencySummaryResponse> BuildCurrencySummaryAsync(string localCurrencyCode, CancellationToken cancellationToken)
    {
        var homeIsoCode2 = _configuration["Budget:AssumedTravelerNationalityIsoCode2"] ?? "BR";
        var homeCurrencyCode = CountryCurrencyLookup.Resolve(homeIsoCode2);

        var rate = await _currencyConverter.ConvertAsync(1m, homeCurrencyCode, localCurrencyCode, cancellationToken);

        var asOfDate = await _dbContext.CurrencyRates
            .Where(r => r.QuoteCurrencyCode == localCurrencyCode)
            .OrderByDescending(r => r.AsOfDate)
            .Select(r => (DateOnly?)r.AsOfDate)
            .FirstOrDefaultAsync(cancellationToken);

        return new CurrencySummaryResponse(rate is not null, homeCurrencyCode, localCurrencyCode, rate, asOfDate);
    }
}
