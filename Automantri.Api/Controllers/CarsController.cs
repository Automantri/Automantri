using Automantri.Application.Cars;
using Microsoft.AspNetCore.Mvc;

namespace Automantri.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CarsController(ICarSearchService carSearchService) : ControllerBase
{
    [HttpPost("import")]
    [ProducesResponseType<IReadOnlyCollection<CarDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CarDto>>> ImportByModel(
        [FromQuery] string model = "camry",
        CancellationToken cancellationToken = default)
    {
        var cars = await carSearchService.ImportByModelAsync(model, cancellationToken);
        return Ok(cars);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<CarDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CarDto>>> GetSavedCars(
        CancellationToken cancellationToken)
    {
        var cars = await carSearchService.GetSavedCarsAsync(cancellationToken);
        return Ok(cars);
    }
}
