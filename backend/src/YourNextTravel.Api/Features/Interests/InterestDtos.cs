using YourNextTravel.Api.Domain.Events;

namespace YourNextTravel.Api.Features.Interests;

public record InterestResponse(Guid Id, InterestCategory Category, string? Detail);

public class CreateInterestRequest
{
    public required InterestCategory Category { get; set; }
    public string? Detail { get; set; }
}
