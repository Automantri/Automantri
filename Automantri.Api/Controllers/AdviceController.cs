using Automantri.Application.Frontend;
using Microsoft.AspNetCore.Mvc;

namespace Automantri.Api.Controllers;

[ApiController]
[Route("api/advice")]
public sealed class AdviceController(IFrontendCarService frontendCarService) : ControllerBase
{
    [HttpPost("sell-or-keep")]
    [ProducesResponseType<SellOrKeepResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SellOrKeepResultDto>> SellOrKeep(
        [FromBody] SellOrKeepRequestDto request)
    {
        var result = await frontendCarService.GetSellOrKeepAdviceAsync(request);
        return Ok(result);
    }

    [HttpPost("repair")]
    [ProducesResponseType<RepairAdviceResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RepairAdviceResultDto>> Repair(
        [FromBody] RepairAdviceRequestDto request)
    {
        var result = await frontendCarService.GetRepairAdviceAsync(request);
        return Ok(result);
    }
}
