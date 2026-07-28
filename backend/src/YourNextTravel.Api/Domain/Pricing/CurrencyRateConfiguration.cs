using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YourNextTravel.Api.Domain.Pricing;

public class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
{
    public void Configure(EntityTypeBuilder<CurrencyRate> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.BaseCurrencyCode).IsRequired().HasMaxLength(3);
        builder.Property(r => r.QuoteCurrencyCode).IsRequired().HasMaxLength(3);
        builder.Property(r => r.Rate).HasPrecision(18, 6);

        builder.HasIndex(r => new { r.BaseCurrencyCode, r.QuoteCurrencyCode, r.AsOfDate })
            .IsUnique();
    }
}
