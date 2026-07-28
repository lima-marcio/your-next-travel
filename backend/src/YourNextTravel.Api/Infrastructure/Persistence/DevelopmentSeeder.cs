using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Users;

namespace YourNextTravel.Api.Infrastructure.Persistence;

/// <summary>
/// Development-only convenience seed: creates one Admin account so the curated-data
/// CRUD endpoints (Features/Admin) are reachable without a manual database edit.
/// </summary>
public static class DevelopmentSeeder
{
    public static async Task SeedAdminUserAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DevelopmentSeeder");

        var adminExists = await dbContext.Users.AnyAsync(u => u.Role == Role.Admin);
        if (adminExists)
        {
            return;
        }

        var email = configuration["Admin:Email"] ?? "admin@yournexttravel.local";
        var password = configuration["Admin:Password"] ?? "Admin123!";

        var passwordHash = passwordHasher.HashPassword(null!, password);
        var admin = User.CreateWithPassword(email, passwordHash, "Admin");
        admin.PromoteToAdmin();

        dbContext.Users.Add(admin);
        dbContext.TravelerProfiles.Add(TravelerProfile.CreateDefault(admin.Id));
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Seeded development admin account: {Email}", email);
    }
}
