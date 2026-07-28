using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNextTravel.Api.Domain.Destinations;

namespace YourNextTravel.Api.Domain.LegalHealth;

public class LegalHealthRequirementConfiguration : IEntityTypeConfiguration<LegalHealthRequirement>
{
    public void Configure(EntityTypeBuilder<LegalHealthRequirement> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.CountryId)
            .IsUnique();

        builder.Property(r => r.VisaRequirementText).IsRequired();
        builder.Property(r => r.VaccinationRequirementText).IsRequired();
        builder.Property(r => r.SourceNote).IsRequired().HasMaxLength(500);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(r => r.CountryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
