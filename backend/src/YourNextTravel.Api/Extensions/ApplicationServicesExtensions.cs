using System.Text.Json.Serialization;
using YourNextTravel.Api.BackgroundServices;
using YourNextTravel.Api.Features.Admin;
using YourNextTravel.Api.Features.Auth;
using YourNextTravel.Api.Features.Discovery;
using YourNextTravel.Api.Features.DestinationGuide;
using YourNextTravel.Api.Features.Interests;
using YourNextTravel.Api.Features.Profiles;
using YourNextTravel.Api.Infrastructure.Currency;
using YourNextTravel.Api.Infrastructure.Destinations;
using YourNextTravel.Api.Infrastructure.Events;
using YourNextTravel.Api.Infrastructure.ExceptionHandling;
using YourNextTravel.Api.Infrastructure.Lodging;
using YourNextTravel.Api.Infrastructure.Persistence;
using YourNextTravel.Api.Infrastructure.Weather;

namespace YourNextTravel.Api.Extensions;

/// <summary>
/// Single composition point for every service the project registers, so
/// Program.cs never references a project service directly.
/// </summary>
public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddCorsPolicy(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddSwaggerWithJwt();
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();
        services.AddPersistence(configuration, environment);
        services.AddAuthFeature();
        services.AddProfilesFeature();
        services.AddInterestsFeature();
        services.AddAdminFeature();
        services.AddDestinationResolver(configuration);
        services.AddWeatherProvider();
        services.AddCurrencyProvider(configuration);
        services.AddLodgingProvider(configuration);
        services.AddEventProviders(configuration);
        services.AddExternalDataRefreshBackgroundService();
        services.AddDestinationGuideFeature();
        services.AddDiscoveryFeature();

        return services;
    }
}
