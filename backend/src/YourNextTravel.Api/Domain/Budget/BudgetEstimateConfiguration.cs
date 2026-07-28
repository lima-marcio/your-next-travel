using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourNextTravel.Api.Domain.Destinations;

namespace YourNextTravel.Api.Domain.Budget;

public class BudgetEstimateConfiguration : IEntityTypeConfiguration<BudgetEstimate>
{
    public void Configure(EntityTypeBuilder<BudgetEstimate> builder)
    {
        builder.HasKey(b => b.Id);

        builder.HasIndex(b => b.DestinationSearchId)
            .IsUnique();

        builder.Property(b => b.LodgingComponentAmount).HasPrecision(12, 2);
        builder.Property(b => b.MiscDailyComponentAmount).HasPrecision(12, 2);
        builder.Property(b => b.TotalAmount).HasPrecision(12, 2);
        builder.Property(b => b.Currency).IsRequired().HasMaxLength(3);
        builder.Property(b => b.AssumptionsNote).IsRequired();

        builder.HasOne<DestinationSearch>()
            .WithOne()
            .HasForeignKey<BudgetEstimate>(b => b.DestinationSearchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
