namespace YourNextTravel.Api.Domain.Events;

public sealed class EventListing
{
    public Guid Id { get; private set; }

    public InterestCategory Category { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Guid? CityId { get; private set; }

    public string? VenueName { get; private set; }

    public DateTime StartUtc { get; private set; }

    public DateTime? EndUtc { get; private set; }

    public string ProviderName { get; private set; } = string.Empty;

    public string ExternalId { get; private set; } = string.Empty;

    public string? ExternalUrl { get; private set; }

    public DateTime FetchedAtUtc { get; private set; }

    private EventListing()
    {
    }

    public static EventListing Create(
        InterestCategory category,
        string title,
        string? description,
        Guid? cityId,
        string? venueName,
        DateTime startUtc,
        DateTime? endUtc,
        string providerName,
        string externalId,
        string? externalUrl)
    {
        return new EventListing
        {
            Id = Guid.NewGuid(),
            Category = category,
            Title = title,
            Description = description,
            CityId = cityId,
            VenueName = venueName,
            StartUtc = startUtc,
            EndUtc = endUtc,
            ProviderName = providerName,
            ExternalId = externalId,
            ExternalUrl = externalUrl,
            FetchedAtUtc = DateTime.UtcNow
        };
    }

    public void RefreshFromSource(
        string title,
        string? description,
        Guid? cityId,
        string? venueName,
        DateTime startUtc,
        DateTime? endUtc,
        string? externalUrl)
    {
        Title = title;
        Description = description;
        CityId = cityId;
        VenueName = venueName;
        StartUtc = startUtc;
        EndUtc = endUtc;
        ExternalUrl = externalUrl;
        FetchedAtUtc = DateTime.UtcNow;
    }
}
