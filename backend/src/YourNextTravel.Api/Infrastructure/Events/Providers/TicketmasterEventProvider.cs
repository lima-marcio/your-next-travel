using System.Text.Json.Serialization;
using YourNextTravel.Api.Domain.Events;

namespace YourNextTravel.Api.Infrastructure.Events.Providers;

public class TicketmasterEventProvider : IEventProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TicketmasterEventProvider> _logger;

    public TicketmasterEventProvider(HttpClient httpClient, IConfiguration configuration, ILogger<TicketmasterEventProvider> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public string ProviderName => EventProviderNames.Ticketmaster;

    public async Task<IReadOnlyList<ExternalEvent>> FetchUpcomingAsync(CancellationToken cancellationToken)
    {
        var apiKey = _configuration["ExternalData:Ticketmaster:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogInformation("Ticketmaster API key is not configured; skipping refresh.");
            return [];
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<DiscoveryResponse>(
                $"events.json?apikey={apiKey}&classificationName=music&size=100&sort=date,asc", cancellationToken);

            var events = response?.Embedded?.Events ?? [];

            return events
                .Where(e => e.Dates?.Start?.DateTime is not null)
                .Select(e => new ExternalEvent(
                    InterestCategory.ConcertsShows,
                    e.Name ?? "Show",
                    null,
                    e.Embedded?.Venues?.FirstOrDefault()?.City?.Name,
                    e.Embedded?.Venues?.FirstOrDefault()?.Name,
                    e.Dates!.Start!.DateTime!.Value,
                    null,
                    e.Id ?? Guid.NewGuid().ToString(),
                    e.Url))
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Ticketmaster lookup failed.");
            return [];
        }
    }

    private sealed class DiscoveryResponse
    {
        [JsonPropertyName("_embedded")]
        public EmbeddedEvents? Embedded { get; set; }
    }

    private sealed class EmbeddedEvents
    {
        [JsonPropertyName("events")]
        public List<TmEvent>? Events { get; set; }
    }

    private sealed class TmEvent
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("dates")]
        public TmDates? Dates { get; set; }

        [JsonPropertyName("_embedded")]
        public TmEmbeddedVenues? Embedded { get; set; }
    }

    private sealed class TmDates
    {
        [JsonPropertyName("start")]
        public TmStart? Start { get; set; }
    }

    private sealed class TmStart
    {
        [JsonPropertyName("dateTime")]
        public DateTime? DateTime { get; set; }
    }

    private sealed class TmEmbeddedVenues
    {
        [JsonPropertyName("venues")]
        public List<TmVenue>? Venues { get; set; }
    }

    private sealed class TmVenue
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("city")]
        public TmCity? City { get; set; }
    }

    private sealed class TmCity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
