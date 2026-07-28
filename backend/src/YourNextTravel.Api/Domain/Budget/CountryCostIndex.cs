namespace YourNextTravel.Api.Domain.Budget;

/// <summary>
/// Curated, per country, scaling a baseline daily non-lodging spend — none of the
/// external sources cover general cost-of-living.
/// </summary>
public sealed class CountryCostIndex
{
    public Guid Id { get; private set; }

    public Guid CountryId { get; private set; }

    public decimal Multiplier { get; private set; }

    private CountryCostIndex()
    {
    }

    public static CountryCostIndex Create(Guid countryId, decimal multiplier)
    {
        if (multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be positive.");
        }

        return new CountryCostIndex
        {
            Id = Guid.NewGuid(),
            CountryId = countryId,
            Multiplier = multiplier
        };
    }

    public void UpdateMultiplier(decimal multiplier)
    {
        if (multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be positive.");
        }

        Multiplier = multiplier;
    }
}
