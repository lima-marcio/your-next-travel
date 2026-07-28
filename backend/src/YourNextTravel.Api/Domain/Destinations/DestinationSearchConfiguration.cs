using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNextTravel.Api.Domain.Users;

namespace YourNextTravel.Api.Domain.Destinations;

public class DestinationSearchConfiguration : IEntityTypeConfiguration<DestinationSearch>
{
    public void Configure(EntityTypeBuilder<DestinationSearch> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TravelerProfileTypeUsed)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Ignore(s => s.Nights);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(s => s.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.UserId, s.CreatedAtUtc });
    }
}
