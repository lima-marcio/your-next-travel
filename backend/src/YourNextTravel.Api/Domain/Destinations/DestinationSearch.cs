using YourNextTravel.Api.Domain.Users;

namespace YourNextTravel.Api.Domain.Destinations;

public sealed class DestinationSearch
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid CityId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public ProfileType TravelerProfileTypeUsed { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private DestinationSearch()
    {
    }

    public static DestinationSearch Create(
        Guid userId,
        Guid cityId,
        DateOnly startDate,
        DateOnly endDate,
        ProfileType travelerProfileTypeUsed)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));
        }

        return new DestinationSearch
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CityId = cityId,
            StartDate = startDate,
            EndDate = endDate,
            TravelerProfileTypeUsed = travelerProfileTypeUsed,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public int Nights => EndDate.DayNumber - StartDate.DayNumber;
}
