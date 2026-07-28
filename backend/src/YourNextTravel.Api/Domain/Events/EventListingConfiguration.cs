using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNextTravel.Api.Domain.Destinations;

namespace YourNextTravel.Api.Domain.Events;

public class EventListingConfiguration : IEntityTypeConfiguration<EventListing>
{
    public void Configure(EntityTypeBuilder<EventListing> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.ProviderName, e.ExternalId })
            .IsUnique();

        builder.Property(e => e.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.VenueName)
            .HasMaxLength(300);

        builder.Property(e => e.ProviderName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ExternalId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ExternalUrl)
            .HasMaxLength(1000);

        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(e => e.CityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.Category, e.StartUtc });
    }
}
