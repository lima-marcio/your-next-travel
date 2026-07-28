namespace YourNextTravel.Api.Domain.Destinations;

public sealed class Country
{
    public Guid Id { get; private set; }

    public string IsoCode2 { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string CurrencyCode { get; private set; } = string.Empty;

    private Country()
    {
    }

    public static Country Create(string isoCode2, string name, string currencyCode)
    {
        return new Country
        {
            Id = Guid.NewGuid(),
            IsoCode2 = isoCode2.ToUpperInvariant(),
            Name = name,
            CurrencyCode = currencyCode.ToUpperInvariant()
        };
    }
}
