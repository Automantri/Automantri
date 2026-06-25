using Automantri.Application.Common.Interfaces;
using Automantri.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Automantri.Application.Cars;

public sealed class CarSyncService(
    IApiNinjasCatalogClient catalogClient,
    IApiNinjasCarsClient carsClient,
    ICarRepository carRepository,
    ICarImageResolver imageResolver,
    IOptions<CarSyncOptions> syncOptions,
    ILogger<CarSyncService> logger) : ICarSyncService
{
    public Task<IReadOnlyCollection<string>> GetAvailableMakesAsync(CancellationToken cancellationToken)
    {
        return catalogClient.GetMakesAsync(cancellationToken);
    }

    public Task<IReadOnlyCollection<string>> GetAvailableModelsAsync(
        string make,
        CancellationToken cancellationToken)
    {
        return catalogClient.GetModelsAsync(CarIdentity.Normalize(make), cancellationToken);
    }

    public async Task<CatalogSyncResultDto> SyncFullCatalogAsync(CancellationToken cancellationToken)
    {
        var makes = await catalogClient.GetMakesAsync(cancellationToken);
        if (makes.Count == 0)
        {
            throw new InvalidOperationException(
                "No car makes are available. API Ninjas /carmakes requires a Business plan, " +
                "or you must configure CarSync:CatalogSeeds in appsettings.json.");
        }

        var insertedCount = 0;
        var updatedCount = 0;
        var modelsProcessed = 0;
        var trimDetailsProcessed = 0;
        var pendingSaves = 0;

        logger.LogInformation("Starting full catalog sync for {MakeCount} makes.", makes.Count);

        foreach (var make in makes)
        {
            var normalizedMake = CarIdentity.Normalize(make);
            var models = await catalogClient.GetModelsAsync(normalizedMake, cancellationToken);
            await DelayAsync(cancellationToken);

            foreach (var model in models)
            {
                if (string.IsNullOrWhiteSpace(model))
                {
                    continue;
                }

                try
                {
                    var normalizedModel = CarIdentity.Normalize(model);
                    var result = await SyncMakeModelInternalAsync(
                        normalizedMake,
                        normalizedModel,
                        cancellationToken,
                        persistChanges: false);

                    insertedCount += result.InsertedCount;
                    updatedCount += result.UpdatedCount;
                    trimDetailsProcessed += result.TrimDetailsProcessed;
                    modelsProcessed++;
                    pendingSaves++;

                    if (pendingSaves >= syncOptions.Value.SaveBatchSize)
                    {
                        await carRepository.SaveChangesAsync(cancellationToken);
                        pendingSaves = 0;
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.LogWarning(
                        exception,
                        "Skipping sync for {Make}/{Model}.",
                        normalizedMake,
                        model);
                }
            }
        }

        if (pendingSaves > 0)
        {
            await carRepository.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation(
            "Completed full catalog sync. Makes={Makes}, Models={Models}, Inserted={Inserted}, Updated={Updated}, TrimDetails={TrimDetails}.",
            makes.Count,
            modelsProcessed,
            insertedCount,
            updatedCount,
            trimDetailsProcessed);

        return new CatalogSyncResultDto(
            makes.Count,
            modelsProcessed,
            insertedCount,
            updatedCount,
            trimDetailsProcessed);
    }

    public async Task<CarSyncResultDto> SyncByMakeModelAsync(
        string make,
        string model,
        CancellationToken cancellationToken)
    {
        var normalizedMake = CarIdentity.Normalize(make);
        var normalizedModel = CarIdentity.Normalize(model);
        var result = await SyncMakeModelInternalAsync(
            normalizedMake,
            normalizedModel,
            cancellationToken,
            persistChanges: true);

        var savedCars = await carRepository.GetAllAsync(cancellationToken);
        var syncedCars = savedCars
            .Where(car =>
                string.Equals(car.Make, normalizedMake, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(car.Model, normalizedModel, StringComparison.OrdinalIgnoreCase))
            .Select(CarDto.FromEntity)
            .ToArray();

        return new CarSyncResultDto(
            normalizedMake,
            normalizedModel,
            result.InsertedCount,
            result.UpdatedCount,
            syncedCars);
    }

    public async Task<IReadOnlyCollection<CarSyncResultDto>> SyncAllConfiguredAsync(
        CancellationToken cancellationToken)
    {
        if (syncOptions.Value.SyncFullCatalog)
        {
            await SyncFullCatalogAsync(cancellationToken);
            return [];
        }

        var targets = syncOptions.Value.Targets;
        if (targets.Count == 0)
        {
            return [];
        }

        var results = new List<CarSyncResultDto>(targets.Count);
        foreach (var target in targets)
        {
            if (string.IsNullOrWhiteSpace(target.Make) || string.IsNullOrWhiteSpace(target.Model))
            {
                continue;
            }

            var result = await SyncByMakeModelAsync(target.Make, target.Model, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    private async Task<MakeModelSyncResult> SyncMakeModelInternalAsync(
        string make,
        string model,
        CancellationToken cancellationToken,
        bool persistChanges)
    {
        var now = DateTimeOffset.UtcNow;
        var carsToUpsert = new List<Car>();

        var fetchedCars = await carsClient.GetCarsAsync(make, model, cancellationToken);
        await DelayAsync(cancellationToken);

        foreach (var car in fetchedCars)
        {
            PrepareCar(car, make, model, $"{make}/{model}", now);
            carsToUpsert.Add(car);
        }

        var trimDetails = await catalogClient.GetTrimDetailsAsync(make, model, cancellationToken);
        await DelayAsync(cancellationToken);

        foreach (var trimDetail in trimDetails)
        {
            var detailCar = MapTrimDetail(trimDetail, make, model, now);
            detailCar.ImageUrl = imageResolver.ResolveImageUrl(make, model, detailCar.Year);
            carsToUpsert.Add(detailCar);
        }

        var upsertResult = await carRepository.UpsertRangeAsync(carsToUpsert, cancellationToken);

        if (persistChanges)
        {
            await carRepository.SaveChangesAsync(cancellationToken);
        }

        return new MakeModelSyncResult(
            upsertResult.InsertedCount,
            upsertResult.UpdatedCount,
            trimDetails.Count);
    }

    private void PrepareCar(Car car, string make, string model, string sourceQuery, DateTimeOffset now)
    {
        car.Make = make;
        car.Model = model;
        car.SourceQuery = sourceQuery;
        car.ImageUrl = imageResolver.ResolveImageUrl(make, model, car.Year);
        car.RetrievedAtUtc = now;
        car.UpdatedAtUtc = now;
        CarIdentity.NormalizeCar(car);
    }

    private static Car MapTrimDetail(
        CarTrimDetails trimDetail,
        string make,
        string model,
        DateTimeOffset now)
    {
        var year = trimDetail.StartProductionYear ?? trimDetail.EndProductionYear ?? 0;
        var car = new Car
        {
            Make = make,
            Model = model,
            Trim = trimDetail.Trim,
            Generation = trimDetail.Generation,
            Serie = trimDetail.Serie,
            CarType = trimDetail.CarType,
            StartProductionYear = trimDetail.StartProductionYear,
            EndProductionYear = trimDetail.EndProductionYear,
            SpecificationsJson = trimDetail.SpecificationsJson,
            Year = year,
            SourceQuery = $"{make}/{model}/{trimDetail.Trim}",
            RetrievedAtUtc = now,
            UpdatedAtUtc = now
        };

        CarIdentity.NormalizeCar(car);
        return car;
    }

    private Task DelayAsync(CancellationToken cancellationToken)
    {
        var delay = syncOptions.Value.RequestDelayMilliseconds;
        if (delay <= 0)
        {
            return Task.CompletedTask;
        }

        return Task.Delay(delay, cancellationToken);
    }

    private sealed record MakeModelSyncResult(
        int InsertedCount,
        int UpdatedCount,
        int TrimDetailsProcessed);
}
