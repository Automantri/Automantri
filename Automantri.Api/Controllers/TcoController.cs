using Automantri.Application.Frontend;
using Microsoft.AspNetCore.Mvc;

namespace Automantri.Api.Controllers;

[ApiController]
[Route("api/tco")]
public sealed class TcoController(IFrontendCarService frontendCarService) : ControllerBase
{
    [HttpPost("calculate")]
    [ProducesResponseType<TcoResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TcoResultDto>> Calculate(
        [FromBody] TcoRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await frontendCarService.CalculateTcoAsync(request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
