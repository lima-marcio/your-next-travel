namespace YourNextTravel.Api.Domain.Budget;

public sealed class BudgetEstimate
{
    public Guid Id { get; private set; }

    public Guid DestinationSearchId { get; private set; }

    public decimal LodgingComponentAmount { get; private set; }

    public decimal MiscDailyComponentAmount { get; private set; }

    public decimal TotalAmount { get; private set; }

    public string Currency { get; private set; } = string.Empty;

    public string AssumptionsNote { get; private set; } = string.Empty;

    public DateTime GeneratedAtUtc { get; private set; }

    private BudgetEstimate()
    {
    }

    public static BudgetEstimate Create(
        Guid destinationSearchId,
        decimal lodgingComponentAmount,
        decimal miscDailyComponentAmount,
        string currency,
        string assumptionsNote)
    {
        return new BudgetEstimate
        {
            Id = Guid.NewGuid(),
            DestinationSearchId = destinationSearchId,
            LodgingComponentAmount = lodgingComponentAmount,
            MiscDailyComponentAmount = miscDailyComponentAmount,
            TotalAmount = lodgingComponentAmount + miscDailyComponentAmount,
            Currency = currency.ToUpperInvariant(),
            AssumptionsNote = assumptionsNote,
            GeneratedAtUtc = DateTime.UtcNow
        };
    }
}
