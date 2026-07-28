using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YourNextTravel.Api.Extensions;

namespace YourNextTravel.Api.Features.Profiles;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfilesController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<ActionResult<TravelerProfileResponse>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _profileService.GetAsync(User.GetUserId(), cancellationToken));
    }

    [HttpPut]
    public async Task<ActionResult<TravelerProfileResponse>> Update(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _profileService.UpdateAsync(User.GetUserId(), request, cancellationToken));
    }
}
