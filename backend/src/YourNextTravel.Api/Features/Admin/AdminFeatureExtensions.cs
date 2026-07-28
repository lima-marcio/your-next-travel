namespace YourNextTravel.Api.Features.Admin;

public static class AdminFeatureExtensions
{
    public static IServiceCollection AddAdminFeature(this IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }
}
