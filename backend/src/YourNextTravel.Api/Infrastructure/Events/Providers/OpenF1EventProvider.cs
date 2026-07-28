using System.Text.Json.Serialization;
using YourNextTravel.Api.Domain.Events;

namespace YourNextTravel.Api.Infrastructure.Events.Providers;

public class OpenF1EventProvider : IEventProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenF1EventProvider> _logger;

    public OpenF1EventProvider(HttpClient httpClient, ILogger<OpenF1EventProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public string ProviderName => EventProviderNames.OpenF1;

    public async Task<IReadOnlyList<ExternalEvent>> FetchUpcomingAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var events = new List<ExternalEvent>();

        foreach (var year in new[] { now.Year, now.Year + 1 })
        {
            try
            {
                var meetings = await _httpClient.GetFromJsonAsync<List<Meeting>>(
                    $"meetings?year={year}", cancellationToken);

                if (meetings is null)
                {
                    continue;
                }

                events.AddRange(meetings
                    .Where(m => m.DateStart is not null && m.DateStart.Value >= now)
                    .Select(m => new ExternalEvent(
                        InterestCategory.MotorsportF1,
                        m.MeetingName ?? "Formula 1 Grand Prix",
                        null,
                        m.Location,
                        m.CircuitShortName,
                        m.DateStart!.Value,
                        null,
                        m.MeetingKey.ToString(),
                        null)));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "OpenF1 lookup failed for year {Year}.", year);
            }
        }

        return events;
    }

    private sealed class Meeting
    {
        [JsonPropertyName("meeting_key")]
        public int MeetingKey { get; set; }

        [JsonPropertyName("meeting_name")]
        public string? MeetingName { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("circuit_short_name")]
        public string? CircuitShortName { get; set; }

        [JsonPropertyName("date_start")]
        public DateTime? DateStart { get; set; }
    }
}
