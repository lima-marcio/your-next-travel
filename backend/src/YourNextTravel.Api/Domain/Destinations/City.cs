namespace YourNextTravel.Api.Domain.Destinations;

public sealed class City
{
    public Guid Id { get; private set; }

    public Guid CountryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }

    private City()
    {
    }

    public static City Create(Guid countryId, string name, double latitude, double longitude)
    {
        return new City
        {
            Id = Guid.NewGuid(),
            CountryId = countryId,
            Name = name,
            Latitude = latitude,
            Longitude = longitude
        };
    }
}
