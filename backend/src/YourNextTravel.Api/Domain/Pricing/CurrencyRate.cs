namespace YourNextTravel.Api.Domain.Pricing;

public sealed class CurrencyRate
{
    public Guid Id { get; private set; }

    public string BaseCurrencyCode { get; private set; } = string.Empty;

    public string QuoteCurrencyCode { get; private set; } = string.Empty;

    public decimal Rate { get; private set; }

    public DateOnly AsOfDate { get; private set; }

    public DateTime FetchedAtUtc { get; private set; }

    private CurrencyRate()
    {
    }

    public static CurrencyRate Create(string baseCurrencyCode, string quoteCurrencyCode, decimal rate, DateOnly asOfDate)
    {
        return new CurrencyRate
        {
            Id = Guid.NewGuid(),
            BaseCurrencyCode = baseCurrencyCode.ToUpperInvariant(),
            QuoteCurrencyCode = quoteCurrencyCode.ToUpperInvariant(),
            Rate = rate,
            AsOfDate = asOfDate,
            FetchedAtUtc = DateTime.UtcNow
        };
    }
}
