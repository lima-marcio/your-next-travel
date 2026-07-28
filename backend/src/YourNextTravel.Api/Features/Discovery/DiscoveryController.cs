using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourNextTravel.Api.Domain.Events;
using YourNextTravel.Api.Extensions;

namespace YourNextTravel.Api.Features.Discovery;

[ApiController]
[Route("api/discovery")]
[Authorize]
public class DiscoveryController : ControllerBase
{
    private readonly IDiscoveryService _discoveryService;

    public DiscoveryController(IDiscoveryService discoveryService)
    {
        _discoveryService = discoveryService;
    }

    [HttpGet("feed")]
    public async Task<ActionResult<DiscoveryFeedResponse>> Feed(CancellationToken cancellationToken)
    {
        return Ok(await _discoveryService.GetFeedAsync(User.GetUserId(), cancellationToken));
    }

    [HttpGet("random-outing")]
    public async Task<ActionResult<RandomOutingResponse>> RandomOuting(TimeHorizon? horizon, CancellationToken cancellationToken)
    {
        return Ok(await _discoveryService.GetRandomOutingAsync(User.GetUserId(), horizon, cancellationToken));
    }
}
