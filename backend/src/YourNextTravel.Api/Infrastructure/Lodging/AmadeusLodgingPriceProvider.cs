using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Destinations;
using YourNextTravel.Api.Domain.Pricing;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Infrastructure.Lodging;

public class AmadeusLodgingPriceProvider : ILodgingPriceProvider
{
    private const int SampleHotelCount = 20;

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AmadeusLodgingPriceProvider> _logger;

    public AmadeusLodgingPriceProvider(
        HttpClient httpClient, IConfiguration configuration, AppDbContext dbContext, ILogger<AmadeusLodgingPriceProvider> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RefreshPriceEstimateAsync(City city, CancellationToken cancellationToken)
    {
        var clientId = _configuration["ExternalData:Amadeus:ClientId"];
        var clientSecret = _configuration["ExternalData:Amadeus:ClientSecret"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            _logger.LogInformation("Amadeus credentials are not configured; skipping lodging price refresh for city {CityId}.", city.Id);
            return;
        }

        var accessToken = await GetAccessTokenAsync(clientId, clientSecret, cancellationToken);
        if (accessToken is null)
        {
            return;
        }

        var hotelIds = await FindNearbyHotelIdsAsync(city, accessToken, cancellationToken);
        if (hotelIds.Count == 0)
        {
            _logger.LogInformation("Amadeus returned no hotels near city {CityId}.", city.Id);
            return;
        }

        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var checkOut = checkIn.AddDays(1);

        var nightlyPrices = await FetchNightlyPricesAsync(hotelIds, checkIn, checkOut, accessToken, cancellationToken);
        if (nightlyPrices.Count == 0)
        {
            _logger.LogInformation("Amadeus returned no bookable offers near city {CityId}.", city.Id);
            return;
        }

        var currency = nightlyPrices[0].Currency;
        var amounts = nightlyPrices.Where(p => p.Currency == currency).Select(p => p.Amount).ToList();

        var estimate = LodgingPriceEstimate.Create(
            city.Id, checkIn, checkOut, amounts.Average(), currency, amounts.Min(), amounts.Max(), amounts.Count);

        _dbContext.LodgingPriceEstimates.Add(estimate);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string?> GetAccessTokenAsync(string clientId, string clientSecret, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "v1/security/oauth2/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret
                })
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
            return payload?.AccessToken;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to obtain an Amadeus access token.");
            return null;
        }
    }

    private async Task<List<string>> FindNearbyHotelIdsAsync(City city, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"v1/reference-data/locations/hotels/by-geocode?latitude={city.Latitude.ToString(CultureInfo.InvariantCulture)}" +
                $"&longitude={city.Longitude.ToString(CultureInfo.InvariantCulture)}&radius=20&radiusUnit=KM");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<HotelListResponse>(cancellationToken);
            return payload?.Data?.Select(h => h.HotelId).Where(id => id is not null).Take(SampleHotelCount).ToList()! ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to look up hotels near city {CityId} via Amadeus.", city.Id);
            return [];
        }
    }

    private async Task<List<(decimal Amount, string Currency)>> FetchNightlyPricesAsync(
        List<string> hotelIds, DateOnly checkIn, DateOnly checkOut, string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"v3/shopping/hotel-offers?hotelIds={string.Join(',', hotelIds)}&adults=1" +
                $"&checkInDate={checkIn:yyyy-MM-dd}&checkOutDate={checkOut:yyyy-MM-dd}&bestRateOnly=true");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<HotelOffersResponse>(cancellationToken);

            return payload?.Data?
                .SelectMany(h => h.Offers ?? [])
                .Where(o => o.Price is not null && decimal.TryParse(o.Price!.Total, CultureInfo.InvariantCulture, out _))
                .Select(o => (decimal.Parse(o.Price!.Total, CultureInfo.InvariantCulture), o.Price.Currency))
                .ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch Amadeus hotel offers.");
            return [];
        }
    }

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private sealed class HotelListResponse
    {
        [JsonPropertyName("data")]
        public List<HotelListItem>? Data { get; set; }
    }

    private sealed class HotelListItem
    {
        [JsonPropertyName("hotelId")]
        public string? HotelId { get; set; }
    }

    private sealed class HotelOffersResponse
    {
        [JsonPropertyName("data")]
        public List<HotelOfferItem>? Data { get; set; }
    }

    private sealed class HotelOfferItem
    {
        [JsonPropertyName("offers")]
        public List<HotelOffer>? Offers { get; set; }
    }

    private sealed class HotelOffer
    {
        [JsonPropertyName("price")]
        public OfferPrice? Price { get; set; }
    }

    private sealed class OfferPrice
    {
        [JsonPropertyName("total")]
        public string? Total { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;
    }
}
