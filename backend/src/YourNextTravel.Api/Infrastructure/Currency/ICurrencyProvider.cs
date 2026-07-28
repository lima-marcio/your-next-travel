namespace YourNextTravel.Api.Infrastructure.Currency;

public interface ICurrencyProvider
{
    /// <summary>
    /// Refreshes USD-pivot exchange rates (Base="USD") for the given currency codes.
    /// All stored rates share USD as the base so that converting between any two of
    /// them is a simple cross-rate calculation.
    /// </summary>
    Task RefreshRatesAsync(IReadOnlyCollection<string> currencyCodes, CancellationToken cancellationToken);
}
