namespace YourNextTravel.Api.Features.Dossier;

public static class DossierFeatureExtensions
{
    public static IServiceCollection AddDossierFeature(this IServiceCollection services)
    {
        services.AddScoped<IBudgetSynthesisService, BudgetSynthesisService>();
        services.AddScoped<IDossierService, DossierService>();
        return services;
    }
}
