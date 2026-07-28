using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNextTravel.Api.Domain.Destinations;

namespace YourNextTravel.Api.Domain.Budget;

public class CountryCostIndexConfiguration : IEntityTypeConfiguration<CountryCostIndex>
{
    public void Configure(EntityTypeBuilder<CountryCostIndex> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.CountryId)
            .IsUnique();

        builder.Property(c => c.Multiplier).HasPrecision(6, 3);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
