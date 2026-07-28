using Microsoft.EntityFrameworkCore;
using YourNextTravel.Api.Domain.Budget;
using YourNextTravel.Api.Domain.Destinations;
using YourNextTravel.Api.Domain.Events;
using YourNextTravel.Api.Domain.LegalHealth;
using YourNextTravel.Api.Domain.Pricing;
using YourNextTravel.Api.Domain.Users;
using YourNextTravel.Api.Domain.Weather;

namespace YourNextTravel.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<TravelerProfile> TravelerProfiles => Set<TravelerProfile>();

    public DbSet<Interest> Interests => Set<Interest>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<DestinationSearch> DestinationSearches => Set<DestinationSearch>();

    public DbSet<LegalHealthRequirement> LegalHealthRequirements => Set<LegalHealthRequirement>();

    public DbSet<EventListing> EventListings => Set<EventListing>();

    public DbSet<LodgingPriceEstimate> LodgingPriceEstimates => Set<LodgingPriceEstimate>();

    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();

    public DbSet<WeatherSnapshot> WeatherSnapshots => Set<WeatherSnapshot>();

    public DbSet<BudgetEstimate> BudgetEstimates => Set<BudgetEstimate>();

    public DbSet<CountryCostIndex> CountryCostIndexes => Set<CountryCostIndex>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
