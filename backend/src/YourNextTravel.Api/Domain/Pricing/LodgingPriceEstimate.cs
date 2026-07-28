namespace YourNextTravel.Api.Domain.Pricing;

/// <summary>
/// A periodic snapshot of typical lodging prices for a city, not an exact quote
/// for a specific stay — the dossier surfaces it as a reference range, not a
/// guaranteed price.
/// </summary>
public sealed class LodgingPriceEstimate
{
    public Guid Id { get; private set; }

    public Guid CityId { get; private set; }

    public DateOnly SampleWindowStart { get; private set; }

    public DateOnly SampleWindowEnd { get; private set; }

    public decimal AvgNightlyAmount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public decimal MinNightlyAmount { get; private set; }

    public decimal MaxNightlyAmount { get; private set; }

    public int SampleSize { get; private set; }

    public DateTime FetchedAtUtc { get; private set; }

    private LodgingPriceEstimate()
    {
    }

    public static LodgingPriceEstimate Create(
        Guid cityId,
        DateOnly sampleWindowStart,
        DateOnly sampleWindowEnd,
        decimal avgNightlyAmount,
        string currency,
        decimal minNightlyAmount,
        decimal maxNightlyAmount,
        int sampleSize)
    {
        return new LodgingPriceEstimate
        {
            Id = Guid.NewGuid(),
            CityId = cityId,
            SampleWindowStart = sampleWindowStart,
            SampleWindowEnd = sampleWindowEnd,
            AvgNightlyAmount = avgNightlyAmount,
            Currency = currency.ToUpperInvariant(),
            MinNightlyAmount = minNightlyAmount,
            MaxNightlyAmount = maxNightlyAmount,
            SampleSize = sampleSize,
            FetchedAtUtc = DateTime.UtcNow
        };
    }
}
