using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Infrastructure.Currency;

public class CurrencyConverter : ICurrencyConverter
{
    private readonly AppDbContext _dbContext;

    public CurrencyConverter(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<decimal?> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken)
    {
        fromCurrency = fromCurrency.ToUpperInvariant();
        toCurrency = toCurrency.ToUpperInvariant();

        if (fromCurrency == toCurrency)
        {
            return amount;
        }

        var usdToFrom = await LatestUsdRateAsync(fromCurrency, cancellationToken);
        var usdToTo = await LatestUsdRateAsync(toCurrency, cancellationToken);

        if (usdToFrom is null || usdToTo is null)
        {
            return null;
        }

        var amountInUsd = amount / usdToFrom.Value;
        return amountInUsd * usdToTo.Value;
    }

    private async Task<decimal?> LatestUsdRateAsync(string currencyCode, CancellationToken cancellationToken)
    {
        if (currencyCode == "USD")
        {
            return 1m;
        }

        return await _dbContext.CurrencyRates
            .Where(r => r.BaseCurrencyCode == "USD" && r.QuoteCurrencyCode == currencyCode)
            .OrderByDescending(r => r.AsOfDate)
            .Select(r => (decimal?)r.Rate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
