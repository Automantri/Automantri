using Automantri.Application.Cars;
using Automantri.Application.Frontend;
using Microsoft.AspNetCore.Mvc;

namespace Automantri.Api.Controllers;

[ApiController]
[Route("api/cars")]
public sealed class CarsController(
    ICarSyncService carSyncService,
    ICarSearchService carSearchService,
    IFrontendCarService frontendCarService) : ControllerBase
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
    [ProducesResponseType<CarSearchResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CarSearchResultDto>> SearchCars(
        [FromQuery] CarSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await frontendCarService.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("raw")]
    [ProducesResponseType<IReadOnlyCollection<CarDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CarDto>>> GetSavedCarsRaw(
        CancellationToken cancellationToken)
    {
        var cars = await carSearchService.GetSavedCarsAsync(cancellationToken);
        return Ok(cars);
    }

    [HttpGet("compare")]
    [ProducesResponseType<IReadOnlyCollection<ComparisonCarDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ComparisonCarDto>>> CompareCars(
        [FromQuery] string ids,
        CancellationToken cancellationToken)
    {
        var carIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (carIds.Length < 2)
        {
            return BadRequest(new { message = "Provide at least two car ids in the ids query parameter." });
        }

        var result = await frontendCarService.CompareAsync(carIds, cancellationToken);
        return Ok(result);
    }

    [HttpGet("recommendations")]
    [ProducesResponseType<IReadOnlyCollection<RecommendationDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<RecommendationDto>>> GetRecommendations(
        [FromQuery] int count = 7,
        CancellationToken cancellationToken = default)
    {
        var result = await frontendCarService.GetRecommendationsAsync(count, cancellationToken);
        return Ok(result);
    }

    [HttpGet("trending")]
    [ProducesResponseType<IReadOnlyCollection<TrendingItemDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<TrendingItemDto>>> GetTrending(
        [FromQuery] int count = 6,
        CancellationToken cancellationToken = default)
    {
        var result = await frontendCarService.GetTrendingAsync(count, cancellationToken);
        return Ok(result);
    }

    [HttpGet("popular-comparisons")]
    [ProducesResponseType<IReadOnlyCollection<PopularComparisonDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<PopularComparisonDto>>> GetPopularComparisons(
        CancellationToken cancellationToken)
    {
        var result = await frontendCarService.GetPopularComparisonsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<CarDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CarDetailDto>> GetCarById(
        string id,
        CancellationToken cancellationToken)
    {
        var car = await frontendCarService.GetDetailByIdAsync(id, cancellationToken);
        return car is null ? NotFound() : Ok(car);
    }

    [HttpGet("{id}/variants")]
    [ProducesResponseType<IReadOnlyCollection<CarVariantDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CarVariantDto>>> GetVariants(
        string id,
        CancellationToken cancellationToken)
    {
        var variants = await frontendCarService.GetVariantsAsync(id, cancellationToken);
        return Ok(variants);
    }

    [HttpGet("{id}/reviews")]
    [ProducesResponseType<IReadOnlyCollection<CarReviewDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CarReviewDto>>> GetReviews(
        string id,
        CancellationToken cancellationToken)
    {
        var reviews = await frontendCarService.GetReviewsAsync(id, cancellationToken);
        return Ok(reviews);
    }

    [HttpGet("{id}/competitors")]
    [ProducesResponseType<IReadOnlyCollection<CarCompetitorDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<CarCompetitorDto>>> GetCompetitors(
        string id,
        CancellationToken cancellationToken)
    {
        var competitors = await frontendCarService.GetCompetitorsAsync(id, cancellationToken);
        return Ok(competitors);
    }
}
