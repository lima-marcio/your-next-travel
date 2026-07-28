using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YourNextTravel.Api.Domain.Destinations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.IsoCode2)
            .IsRequired()
            .HasMaxLength(2);

        builder.HasIndex(c => c.IsoCode2)
            .IsUnique();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3);
    }
}
