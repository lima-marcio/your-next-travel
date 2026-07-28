using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace YourNextTravel.Api.Features.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("legal-health")]
    public async Task<ActionResult<IReadOnlyList<LegalHealthRequirementResponse>>> ListLegalHealth(CancellationToken cancellationToken)
    {
        return Ok(await _adminService.ListLegalHealthRequirementsAsync(cancellationToken));
    }

    [HttpPut("legal-health")]
    public async Task<ActionResult<LegalHealthRequirementResponse>> UpsertLegalHealth(
        UpsertLegalHealthRequirementRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _adminService.UpsertLegalHealthRequirementAsync(request, cancellationToken));
    }

    [HttpGet("curated-events")]
    public async Task<ActionResult<IReadOnlyList<CuratedEventResponse>>> ListCuratedEvents(CancellationToken cancellationToken)
    {
        return Ok(await _adminService.ListCuratedEventsAsync(cancellationToken));
    }

    [HttpPost("curated-events")]
    public async Task<ActionResult<CuratedEventResponse>> CreateCuratedEvent(
        CreateCuratedEventRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _adminService.CreateCuratedEventAsync(request, cancellationToken));
    }

    [HttpDelete("curated-events/{eventId:guid}")]
    public async Task<IActionResult> DeleteCuratedEvent(Guid eventId, CancellationToken cancellationToken)
    {
        await _adminService.DeleteCuratedEventAsync(eventId, cancellationToken);
        return NoContent();
    }
}
