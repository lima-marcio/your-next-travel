namespace YourNextTravel.Api.Domain.Weather;

/// <summary>
/// Two granularities are needed because Open-Meteo's forecast horizon covers only
/// ~16 days: MonthlyClimatology (historical monthly normals) drives "best time to
/// visit" year-round, while DailyForecast is only available/shown when the trip
/// start falls within the forecast horizon.
/// </summary>
public sealed class WeatherSnapshot
{
    public Guid Id { get; private set; }

    public Guid CityId { get; private set; }

    public WeatherGranularity Granularity { get; private set; }

    public int? Month { get; private set; }

    public DateOnly? ForDate { get; private set; }

    public double AvgTempC { get; private set; }

    public double MinTempC { get; private set; }

    public double MaxTempC { get; private set; }

    public double PrecipitationMm { get; private set; }

    public DateTime FetchedAtUtc { get; private set; }

    private WeatherSnapshot()
    {
    }

    public static WeatherSnapshot CreateClimatology(
        Guid cityId, int month, double avgTempC, double minTempC, double maxTempC, double precipitationMm)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        return new WeatherSnapshot
        {
            Id = Guid.NewGuid(),
            CityId = cityId,
            Granularity = WeatherGranularity.MonthlyClimatology,
            Month = month,
            AvgTempC = avgTempC,
            MinTempC = minTempC,
            MaxTempC = maxTempC,
            PrecipitationMm = precipitationMm,
            FetchedAtUtc = DateTime.UtcNow
        };
    }

    public static WeatherSnapshot CreateForecast(
        Guid cityId, DateOnly forDate, double avgTempC, double minTempC, double maxTempC, double precipitationMm)
    {
        return new WeatherSnapshot
        {
            Id = Guid.NewGuid(),
            CityId = cityId,
            Granularity = WeatherGranularity.DailyForecast,
            ForDate = forDate,
            AvgTempC = avgTempC,
            MinTempC = minTempC,
            MaxTempC = maxTempC,
            PrecipitationMm = precipitationMm,
            FetchedAtUtc = DateTime.UtcNow
        };
    }
}
