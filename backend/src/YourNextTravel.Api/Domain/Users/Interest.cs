using YourNextTravel.Api.Domain.Events;

namespace YourNextTravel.Api.Domain.Users;

public sealed class Interest
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public InterestCategory Category { get; private set; }

    public string? Detail { get; private set; }

    private Interest()
    {
    }

    public static Interest Create(Guid userId, InterestCategory category, string? detail = null)
    {
        return new Interest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Category = category,
            Detail = detail
        };
    }
}
