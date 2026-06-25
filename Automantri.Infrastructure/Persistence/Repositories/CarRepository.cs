using Automantri.Application.Cars;
using Automantri.Application.Common.Interfaces;
using Automantri.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Automantri.Infrastructure.Persistence.Repositories;

internal sealed class CarRepository(AutomantriDbContext dbContext) : ICarRepository
{
    public async Task<CarUpsertResult> UpsertRangeAsync(
        IReadOnlyCollection<Car> cars,
        CancellationToken cancellationToken)
    {
        if (cars.Count == 0)
        {
            return new CarUpsertResult(0, 0);
        }

        var makeKeys = cars.Select(car => car.Make.ToLower()).Distinct().ToArray();
        var modelKeys = cars.Select(car => car.Model.ToLower()).Distinct().ToArray();

        var existingCars = await dbContext.Cars
            .Where(car => makeKeys.Contains(car.Make.ToLower()) && modelKeys.Contains(car.Model.ToLower()))
            .ToListAsync(cancellationToken);

        var existingByKey = existingCars.ToDictionary(CarIdentity.BuildKey);
        var insertedCount = 0;
        var updatedCount = 0;

        foreach (var car in cars)
        {
            var key = CarIdentity.BuildKey(car);
            if (existingByKey.TryGetValue(key, out var existing))
            {
                ApplyValues(existing, car);
                updatedCount++;
                continue;
            }

            car.Id = Guid.NewGuid();
            await dbContext.Cars.AddAsync(car, cancellationToken);
            existingByKey[key] = car;
            insertedCount++;
        }

        return new CarUpsertResult(insertedCount, updatedCount);
    }

    public async Task<IReadOnlyCollection<Car>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Cars
            .AsNoTracking()
            .OrderByDescending(car => car.UpdatedAtUtc)
            .ThenBy(car => car.Make)
            .ThenBy(car => car.Model)
            .ThenBy(car => car.Year)
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyValues(Car existing, Car incoming)
    {
        existing.CityMpg = incoming.CityMpg;
        existing.VehicleClass = incoming.VehicleClass;
        existing.CombinationMpg = incoming.CombinationMpg;
        existing.Cylinders = incoming.Cylinders;
        existing.Displacement = incoming.Displacement;
        existing.Drive = incoming.Drive;
        existing.FuelType = incoming.FuelType;
        existing.HighwayMpg = incoming.HighwayMpg;
        existing.Transmission = incoming.Transmission;
        existing.Year = incoming.Year;
        existing.Trim = incoming.Trim;
        existing.Generation = incoming.Generation;
        existing.Serie = incoming.Serie;
        existing.CarType = incoming.CarType;
        existing.StartProductionYear = incoming.StartProductionYear;
        existing.EndProductionYear = incoming.EndProductionYear;
        existing.SpecificationsJson = incoming.SpecificationsJson;
        existing.SourceQuery = incoming.SourceQuery;
        existing.ImageUrl = incoming.ImageUrl;
        existing.RetrievedAtUtc = incoming.RetrievedAtUtc;
        existing.UpdatedAtUtc = incoming.UpdatedAtUtc;
    }
}
