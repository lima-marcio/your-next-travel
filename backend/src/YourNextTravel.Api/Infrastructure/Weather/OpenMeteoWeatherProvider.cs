using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Destinations;
using YourNextTravel.Api.Domain.Weather;
using YourNextTravel.Api.Infrastructure.Persistence;

namespace YourNextTravel.Api.Infrastructure.Weather;

public class OpenMeteoWeatherProvider : IWeatherProvider
{
    private const string DailyParams = "temperature_2m_mean,temperature_2m_max,temperature_2m_min,precipitation_sum";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OpenMeteoWeatherProvider> _logger;

    public OpenMeteoWeatherProvider(
        HttpClient httpClient, IConfiguration configuration, AppDbContext dbContext, ILogger<OpenMeteoWeatherProvider> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task RefreshClimatologyAsync(City city, CancellationToken cancellationToken)
    {
        var archiveBaseUrl = _configuration["ExternalData:OpenMeteo:ArchiveBaseUrl"]
            ?? throw new InvalidOperationException("ExternalData:OpenMeteo:ArchiveBaseUrl is not configured.");

        var lastCompleteYear = DateTime.UtcNow.Year - 1;
        var startDate = new DateOnly(lastCompleteYear, 1, 1);
        var endDate = new DateOnly(lastCompleteYear, 12, 31);

        var url = $"{archiveBaseUrl}/archive?latitude={city.Latitude.ToString(CultureInfo.InvariantCulture)}" +
                  $"&longitude={city.Longitude.ToString(CultureInfo.InvariantCulture)}" +
                  $"&start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}" +
                  $"&daily={DailyParams}&timezone=UTC";

        DailyWeatherResponse? response;
        try
        {
            response = await _httpClient.GetFromJsonAsync<DailyWeatherResponse>(url, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Open-Meteo archive lookup failed for city {CityId}.", city.Id);
            return;
        }

        var daily = response?.Daily;
        if (daily?.Time is null)
        {
            return;
        }

        var byMonth = daily.Time
            .Select((date, index) => new
            {
                Month = DateOnly.Parse(date, CultureInfo.InvariantCulture).Month,
                Mean = daily.TemperatureMean?[index],
                Max = daily.TemperatureMax?[index],
                Min = daily.TemperatureMin?[index],
                Precipitation = daily.PrecipitationSum?[index]
            })
            .Where(d => d.Mean.HasValue && d.Max.HasValue && d.Min.HasValue)
            .GroupBy(d => d.Month);

        foreach (var monthGroup in byMonth)
        {
            var avgTemp = monthGroup.Average(d => d.Mean!.Value);
            var avgMax = monthGroup.Average(d => d.Max!.Value);
            var avgMin = monthGroup.Average(d => d.Min!.Value);
            var avgPrecip = monthGroup.Average(d => d.Precipitation ?? 0);

            var existing = await _dbContext.WeatherSnapshots.FirstOrDefaultAsync(
                w => w.CityId == city.Id && w.Granularity == WeatherGranularity.MonthlyClimatology && w.Month == monthGroup.Key,
                cancellationToken);

            if (existing is not null)
            {
                _dbContext.WeatherSnapshots.Remove(existing);
            }

            _dbContext.WeatherSnapshots.Add(
                WeatherSnapshot.CreateClimatology(city.Id, monthGroup.Key, avgTemp, avgMin, avgMax, avgPrecip));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RefreshForecastAsync(City city, CancellationToken cancellationToken)
    {
        var forecastBaseUrl = _configuration["ExternalData:OpenMeteo:ForecastBaseUrl"]
            ?? throw new InvalidOperationException("ExternalData:OpenMeteo:ForecastBaseUrl is not configured.");

        var url = $"{forecastBaseUrl}/forecast?latitude={city.Latitude.ToString(CultureInfo.InvariantCulture)}" +
                  $"&longitude={city.Longitude.ToString(CultureInfo.InvariantCulture)}" +
                  $"&daily={DailyParams}&forecast_days=16&timezone=UTC";

        DailyWeatherResponse? response;
        try
        {
            response = await _httpClient.GetFromJsonAsync<DailyWeatherResponse>(url, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Open-Meteo forecast lookup failed for city {CityId}.", city.Id);
            return;
        }

        var daily = response?.Daily;
        if (daily?.Time is null)
        {
            return;
        }

        var existingForecasts = await _dbContext.WeatherSnapshots
            .Where(w => w.CityId == city.Id && w.Granularity == WeatherGranularity.DailyForecast)
            .ToListAsync(cancellationToken);
        _dbContext.WeatherSnapshots.RemoveRange(existingForecasts);

        for (var i = 0; i < daily.Time.Count; i++)
        {
            if (daily.TemperatureMean?[i] is null || daily.TemperatureMax?[i] is null || daily.TemperatureMin?[i] is null)
            {
                continue;
            }

            var forDate = DateOnly.Parse(daily.Time[i], CultureInfo.InvariantCulture);
            _dbContext.WeatherSnapshots.Add(WeatherSnapshot.CreateForecast(
                city.Id, forDate, daily.TemperatureMean[i]!.Value, daily.TemperatureMin[i]!.Value,
                daily.TemperatureMax[i]!.Value, daily.PrecipitationSum?[i] ?? 0));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class DailyWeatherResponse
    {
        [JsonPropertyName("daily")]
        public DailyBlock? Daily { get; set; }
    }

    private sealed class DailyBlock
    {
        [JsonPropertyName("time")]
        public List<string>? Time { get; set; }

        [JsonPropertyName("temperature_2m_mean")]
        public List<double?>? TemperatureMean { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public List<double?>? TemperatureMax { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public List<double?>? TemperatureMin { get; set; }

        [JsonPropertyName("precipitation_sum")]
        public List<double?>? PrecipitationSum { get; set; }
    }
}
