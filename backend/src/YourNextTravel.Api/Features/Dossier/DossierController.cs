using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourNextTravel.Api.Extensions;

namespace YourNextTravel.Api.Features.Dossier;

[ApiController]
[Route("api/dossier")]
[Authorize]
public class DossierController : ControllerBase
{
    private readonly IDossierService _dossierService;

    public DossierController(IDossierService dossierService)
    {
        _dossierService = dossierService;
    }

    [HttpPost("search")]
    public async Task<ActionResult<DossierResponse>> Search(DossierSearchRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _dossierService.SearchAsync(User.GetUserId(), request, cancellationToken));
    }

    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<DestinationSearchSummaryResponse>>> History(CancellationToken cancellationToken)
    {
        return Ok(await _dossierService.GetHistoryAsync(User.GetUserId(), cancellationToken));
    }

    [HttpGet("{searchId:guid}")]
    public async Task<ActionResult<DossierResponse>> GetBySearchId(Guid searchId, CancellationToken cancellationToken)
    {
        return Ok(await _dossierService.GetBySearchIdAsync(User.GetUserId(), searchId, cancellationToken));
    }
}
