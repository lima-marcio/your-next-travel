namespace YourNextTravel.Api.Domain.LegalHealth;

/// <summary>
/// Curated manually, per country, for a single assumed traveler nationality
/// (see Budget:AssumedTravelerNationalityIsoCode2 in configuration) rather than
/// a full nationality-by-destination visa matrix.
/// </summary>
public sealed class LegalHealthRequirement
{
    public Guid Id { get; private set; }

    public Guid CountryId { get; private set; }

    public string VisaRequirementText { get; private set; } = string.Empty;

    public string VaccinationRequirementText { get; private set; } = string.Empty;

    public string? OtherHealthNotes { get; private set; }

    public string SourceNote { get; private set; } = string.Empty;

    public DateTime LastReviewedAtUtc { get; private set; }

    private LegalHealthRequirement()
    {
    }

    public static LegalHealthRequirement Create(
        Guid countryId,
        string visaRequirementText,
        string vaccinationRequirementText,
        string? otherHealthNotes,
        string sourceNote)
    {
        return new LegalHealthRequirement
        {
            Id = Guid.NewGuid(),
            CountryId = countryId,
            VisaRequirementText = visaRequirementText,
            VaccinationRequirementText = vaccinationRequirementText,
            OtherHealthNotes = otherHealthNotes,
            SourceNote = sourceNote,
            LastReviewedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(
        string visaRequirementText,
        string vaccinationRequirementText,
        string? otherHealthNotes,
        string sourceNote)
    {
        VisaRequirementText = visaRequirementText;
        VaccinationRequirementText = vaccinationRequirementText;
        OtherHealthNotes = otherHealthNotes;
        SourceNote = sourceNote;
        LastReviewedAtUtc = DateTime.UtcNow;
    }
}
