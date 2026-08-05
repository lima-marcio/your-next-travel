using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourNextTravel.Api.Extensions;

namespace YourNextTravel.Api.Features.DestinationGuide;

[ApiController]
[Route("api/destination-guide")]
[Authorize]
public class DestinationGuideController : ControllerBase
{
    private readonly IDestinationGuideService _destinationGuideService;

    public DestinationGuideController(IDestinationGuideService destinationGuideService)
    {
        _destinationGuideService = destinationGuideService;
    }

    [HttpPost("search")]
    public async Task<ActionResult<DestinationGuideResponse>> Search(DestinationGuideSearchRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _destinationGuideService.SearchAsync(User.GetUserId(), request, cancellationToken));
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<DestinationSearchSummaryResponse>>> History(CancellationToken cancellationToken)
    {
        return Ok(await _destinationGuideService.GetHistoryAsync(User.GetUserId(), cancellationToken));
    }

    [HttpGet("{searchId:guid}")]
    public async Task<ActionResult<DestinationGuideResponse>> GetBySearchId(Guid searchId, CancellationToken cancellationToken)
    {
        return Ok(await _destinationGuideService.GetBySearchIdAsync(User.GetUserId(), searchId, cancellationToken));
    }
}
