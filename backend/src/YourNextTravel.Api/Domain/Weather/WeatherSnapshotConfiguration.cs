using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNextTravel.Api.Domain.Destinations;

namespace YourNextTravel.Api.Domain.Weather;

public class WeatherSnapshotConfiguration : IEntityTypeConfiguration<WeatherSnapshot>
{
    public void Configure(EntityTypeBuilder<WeatherSnapshot> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Granularity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(w => w.CityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.CityId, w.Granularity, w.Month });
        builder.HasIndex(w => new { w.CityId, w.Granularity, w.ForDate });
    }
}
