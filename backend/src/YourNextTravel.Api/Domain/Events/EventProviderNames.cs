namespace YourNextTravel.Api.Domain.Events;

/// <summary>
/// Known values for <see cref="EventListing.ProviderName"/>. Adding a new source
/// (another curated category, another API) only requires a new constant here and a
/// new IEventProvider implementation — no changes to EventListing or its consumers.
/// </summary>
public static class EventProviderNames
{
    public const string FootballData = "football-data.org";
    public const string OpenF1 = "OpenF1";
    public const string Ticketmaster = "Ticketmaster";
    public const string Curated = "Curated";
}
