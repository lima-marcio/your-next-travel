using YourNextTravel.Api.Domain.Destinations;

namespace YourNextTravel.Api.Infrastructure.Weather;

public interface IWeatherProvider
{
    /// <summary>Refreshes the 12 MonthlyClimatology snapshots (drives "best time to visit").</summary>
    Task RefreshClimatologyAsync(City city, CancellationToken cancellationToken);

    /// <summary>Refreshes DailyForecast snapshots for the ~16-day forecast horizon.</summary>
    Task RefreshForecastAsync(City city, CancellationToken cancellationToken);
}
