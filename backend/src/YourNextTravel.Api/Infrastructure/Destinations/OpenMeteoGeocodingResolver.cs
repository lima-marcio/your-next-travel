using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Destinations;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Infrastructure.Destinations;

public class OpenMeteoGeocodingResolver : IDestinationResolver
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OpenMeteoGeocodingResolver> _logger;

    public OpenMeteoGeocodingResolver(HttpClient httpClient, AppDbContext dbContext, ILogger<OpenMeteoGeocodingResolver> logger)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<City?> ResolveAsync(string freeTextQuery, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Cities
            .FirstOrDefaultAsync(c => c.Name.ToLower() == freeTextQuery.Trim().ToLower(), cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        GeocodingResponse? response;
        try
        {
            response = await _httpClient.GetFromJsonAsync<GeocodingResponse>(
                $"search?name={Uri.EscapeDataString(freeTextQuery)}&count=1&language=en&format=json",
                cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Open-Meteo geocoding lookup failed for query '{Query}'.", freeTextQuery);
            return null;
        }

        var result = response?.Results?.FirstOrDefault();
        if (result is null)
        {
            return null;
        }

        var country = await _dbContext.Countries
            .FirstOrDefaultAsync(c => c.IsoCode2 == result.CountryCode, cancellationToken);

        if (country is null)
        {
            country = Country.Create(result.CountryCode, result.Country, CountryCurrencyLookup.Resolve(result.CountryCode));
            _dbContext.Countries.Add(country);
        }

        var city = City.Create(country.Id, result.Name, result.Latitude, result.Longitude);
        _dbContext.Cities.Add(city);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return city;
    }

    private sealed class GeocodingResponse
    {
        [JsonPropertyName("results")]
        public List<GeocodingResult>? Results { get; set; }
    }

    private sealed class GeocodingResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }
}
