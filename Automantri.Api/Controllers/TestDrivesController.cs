using Automantri.Application.Frontend;
using Microsoft.AspNetCore.Mvc;

namespace Automantri.Api.Controllers;

[ApiController]
[Route("api/test-drives")]
public sealed class TestDrivesController(IFrontendCarService frontendCarService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TestDriveResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TestDriveResultDto>> BookTestDrive(
        [FromBody] TestDriveRequestDto request)
    {
        var result = await frontendCarService.BookTestDriveAsync(request);
        return Ok(result);
    }
}
