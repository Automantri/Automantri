using Automantri.Application.Cars;
using Microsoft.AspNetCore.Mvc;

namespace Automantri.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CarsController(
    ICarSyncService carSyncService,
    ICarSearchService carSearchService) : ControllerBase
{
    [HttpGet("makes")]
    [ProducesResponseType<IReadOnlyCollection<string>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<string>>> GetAvailableMakes(
        CancellationToken cancellationToken)
    {
        var makes = await carSyncService.GetAvailableMakesAsync(cancellationToken);
        return Ok(makes);
    }

    [HttpGet("makes/{make}/models")]
    [ProducesResponseType<IReadOnlyCollection<string>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<string>>> GetAvailableModels(
        string make,
        CancellationToken cancellationToken)
    {
        var models = await carSyncService.GetAvailableModelsAsync(make, cancellationToken);
        return Ok(models);
    }

    [HttpPost("sync/catalog")]
    [ProducesResponseType<CatalogSyncResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CatalogSyncResultDto>> SyncFullCatalog(
        CancellationToken cancellationToken)
    {
        var result = await carSyncService.SyncFullCatalogAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("sync")]
    [ProducesResponseType<CarSyncResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CarSyncResultDto>> SyncByMakeModel(
        [FromQuery] string make = "toyota",
        [FromQuery] string model = "camry",
        CancellationToken cancellationToken = default)
    {
        var result = await carSyncService.SyncByMakeModelAsync(make, model, cancellationToken);
        return Ok(result);
    }

    [HttpPost("import")]
    [ProducesResponseType<CarSyncResultDto>(StatusCodes.Status200OK)]
    [Obsolete("Use POST /api/cars/sync instead.")]
    public Task<ActionResult<CarSyncResultDto>> ImportByModel(
        [FromQuery] string make = "toyota",
        [FromQuery] string model = "camry",
        CancellationToken cancellationToken = default)
    {
        return SyncByMakeModel(make, model, cancellationToken);
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
