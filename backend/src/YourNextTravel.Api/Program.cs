using Microsoft.EntityFrameworkCore;
using Serilog;
using YourNextTravel.Api.Extensions;
using YourNextTravel.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();

    if (builder.Configuration.GetValue("Seed:AdminUser", false))
    {
        await DevelopmentSeeder.SeedAdminUserAsync(scope.ServiceProvider);
    }
    else
    {
        app.Logger.LogInformation(
            "Skipping development admin seed — set Seed:AdminUser=true (e.g. via dotnet user-secrets) to confirm it explicitly.");
    }
}

app.UseHttpsRedirection();
app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
