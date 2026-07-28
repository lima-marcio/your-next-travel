namespace YourNextTravel.Api.Infrastructure.Currency;

public interface ICurrencyConverter
{
    /// <summary>
    /// Converts an amount between two currencies using the cached USD-pivot rates
    /// (see FrankfurterCurrencyProvider). Returns null if either currency's rate has
    /// not been cached yet.
    /// </summary>
    Task<decimal?> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken);
}
