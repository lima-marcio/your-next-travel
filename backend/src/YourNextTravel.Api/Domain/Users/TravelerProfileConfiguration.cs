using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YourNextTravel.Api.Domain.Users;

public class TravelerProfileConfiguration : IEntityTypeConfiguration<TravelerProfile>
{
    public void Configure(EntityTypeBuilder<TravelerProfile> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.Property(p => p.ProfileType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<TravelerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
