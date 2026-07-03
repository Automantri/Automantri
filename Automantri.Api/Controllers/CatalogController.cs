using Automantri.Application.Frontend;
using Microsoft.AspNetCore.Mvc;

namespace Automantri.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController(IFrontendCarService frontendCarService) : ControllerBase
{
    [HttpGet("brands")]
    [ProducesResponseType<IReadOnlyCollection<BrandDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<BrandDto>>> GetBrands(
        CancellationToken cancellationToken)
    {
        var brands = await frontendCarService.GetBrandsAsync(cancellationToken);
        return Ok(brands);
    }
}
