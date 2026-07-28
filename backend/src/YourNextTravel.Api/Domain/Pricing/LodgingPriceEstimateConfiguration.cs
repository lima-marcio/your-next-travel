using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNextTravel.Api.Domain.Destinations;

namespace YourNextTravel.Api.Domain.Pricing;

public class LodgingPriceEstimateConfiguration : IEntityTypeConfiguration<LodgingPriceEstimate>
{
    public void Configure(EntityTypeBuilder<LodgingPriceEstimate> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.AvgNightlyAmount).HasPrecision(12, 2);
        builder.Property(e => e.MinNightlyAmount).HasPrecision(12, 2);
        builder.Property(e => e.MaxNightlyAmount).HasPrecision(12, 2);

        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(e => e.CityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CityId, e.FetchedAtUtc });
    }
}
