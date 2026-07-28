using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Pricing;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Infrastructure.Currency;

public class FrankfurterCurrencyProvider : ICurrencyProvider
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<FrankfurterCurrencyProvider> _logger;

    public FrankfurterCurrencyProvider(HttpClient httpClient, AppDbContext dbContext, ILogger<FrankfurterCurrencyProvider> logger)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RefreshRatesAsync(IReadOnlyCollection<string> currencyCodes, CancellationToken cancellationToken)
    {
        var symbols = currencyCodes
            .Select(c => c.ToUpperInvariant())
            .Where(c => c != "USD")
            .Distinct()
            .ToList();

        if (symbols.Count == 0)
        {
            return;
        }

        FrankfurterResponse? response;
        try
        {
            response = await _httpClient.GetFromJsonAsync<FrankfurterResponse>(
                $"latest?base=USD&symbols={string.Join(',', symbols)}", cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Frankfurter currency lookup failed for symbols {Symbols}.", string.Join(',', symbols));
            return;
        }

        if (response?.Rates is null)
        {
            return;
        }

        var asOfDate = DateOnly.Parse(response.Date, CultureInfo.InvariantCulture);

        await UpsertRateAsync("USD", "USD", 1m, asOfDate, cancellationToken);

        foreach (var (quoteCurrency, rate) in response.Rates)
        {
            await UpsertRateAsync("USD", quoteCurrency, rate, asOfDate, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertRateAsync(string baseCurrency, string quoteCurrency, decimal rate, DateOnly asOfDate, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.CurrencyRates.FirstOrDefaultAsync(
            r => r.BaseCurrencyCode == baseCurrency && r.QuoteCurrencyCode == quoteCurrency && r.AsOfDate == asOfDate,
            cancellationToken);

        if (existing is not null)
        {
            return;
        }

        _dbContext.CurrencyRates.Add(CurrencyRate.Create(baseCurrency, quoteCurrency, rate, asOfDate));
    }

    private sealed class FrankfurterResponse
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
