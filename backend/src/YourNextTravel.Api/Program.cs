using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using YourNextTravel.Api.BackgroundServices;
using YourNextTravel.Api.Extensions;
using YourNextTravel.Api.Features.Admin;
using YourNextTravel.Api.Features.Auth;
using YourNextTravel.Api.Features.Discovery;
using YourNextTravel.Api.Features.Dossier;
using YourNextTravel.Api.Features.Interests;
using YourNextTravel.Api.Features.Profiles;
using YourNextTravel.Api.Infrastructure.Currency;
using YourNextTravel.Api.Infrastructure.Destinations;
using YourNextTravel.Api.Infrastructure.Events;
using YourNextTravel.Api.Infrastructure.ExceptionHandling;
using YourNextTravel.Api.Infrastructure.Lodging;
using YourNextTravel.Api.Infrastructure.Persistence;
using YourNextTravel.Api.Infrastructure.Weather;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerWithJwt();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddPersistence(builder.Configuration, builder.Environment);
builder.Services.AddAuthFeature();
builder.Services.AddProfilesFeature();
builder.Services.AddInterestsFeature();
builder.Services.AddAdminFeature();
builder.Services.AddDestinationResolver(builder.Configuration);
builder.Services.AddWeatherProvider();
builder.Services.AddCurrencyProvider(builder.Configuration);
builder.Services.AddLodgingProvider(builder.Configuration);
builder.Services.AddEventProviders(builder.Configuration);
builder.Services.AddExternalDataRefreshBackgroundService();
builder.Services.AddDossierFeature();
builder.Services.AddDiscoveryFeature();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    await DevelopmentSeeder.SeedAdminUserAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();
app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
