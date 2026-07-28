using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourNextTravel.Api.Extensions;

namespace YourNextTravel.Api.Features.Interests;

[ApiController]
[Route("api/interests")]
[Authorize]
public class InterestsController : ControllerBase
{
    private readonly IInterestService _interestService;

    public InterestsController(IInterestService interestService)
    {
        _interestService = interestService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InterestResponse>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _interestService.ListAsync(User.GetUserId(), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<InterestResponse>> Add(CreateInterestRequest request, CancellationToken cancellationToken)
    {
        var result = await _interestService.AddAsync(User.GetUserId(), request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{interestId:guid}")]
    public async Task<IActionResult> Remove(Guid interestId, CancellationToken cancellationToken)
    {
        await _interestService.RemoveAsync(User.GetUserId(), interestId, cancellationToken);
        return NoContent();
    }
}
