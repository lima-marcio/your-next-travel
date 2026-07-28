using System.Text.Json.Serialization;
using YourNextTravel.Api.Domain.Events;

namespace YourNextTravel.Api.Infrastructure.Events.Providers;

public class FootballDataEventProvider : IEventProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FootballDataEventProvider> _logger;

    public FootballDataEventProvider(HttpClient httpClient, IConfiguration configuration, ILogger<FootballDataEventProvider> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public string ProviderName => EventProviderNames.FootballData;

    public async Task<IReadOnlyList<ExternalEvent>> FetchUpcomingAsync(CancellationToken cancellationToken)
    {
        var apiToken = _configuration["ExternalData:FootballData:ApiToken"];
        if (string.IsNullOrWhiteSpace(apiToken))
        {
            _logger.LogInformation("football-data.org API token is not configured; skipping refresh.");
            return [];
        }

        var dateFrom = DateOnly.FromDateTime(DateTime.UtcNow);
        var dateTo = dateFrom.AddMonths(6);

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, $"matches?dateFrom={dateFrom:yyyy-MM-dd}&dateTo={dateTo:yyyy-MM-dd}");
            request.Headers.Add("X-Auth-Token", apiToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<MatchesResponse>(cancellationToken);

            return payload?.Matches?
                .Where(m => m.UtcDate is not null)
                .Select(m => new ExternalEvent(
                    InterestCategory.Football,
                    $"{m.HomeTeam?.Name ?? "TBD"} vs {m.AwayTeam?.Name ?? "TBD"} ({m.Competition?.Name})",
                    null,
                    null,
                    m.Venue,
                    m.UtcDate!.Value,
                    null,
                    m.Id.ToString(),
                    null))
                .ToList() ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "football-data.org lookup failed.");
            return [];
        }
    }

    private sealed class MatchesResponse
    {
        [JsonPropertyName("matches")]
        public List<Match>? Matches { get; set; }
    }

    private sealed class Match
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("utcDate")]
        public DateTime? UtcDate { get; set; }

        [JsonPropertyName("venue")]
        public string? Venue { get; set; }

        [JsonPropertyName("homeTeam")]
        public Team? HomeTeam { get; set; }

        [JsonPropertyName("awayTeam")]
        public Team? AwayTeam { get; set; }

        [JsonPropertyName("competition")]
        public Competition? Competition { get; set; }
    }

    private sealed class Team
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class Competition
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
