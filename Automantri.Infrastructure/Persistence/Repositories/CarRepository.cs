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

    public async Task<Car?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Cars
            .AsNoTracking()
            .FirstOrDefaultAsync(car => car.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Car>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await dbContext.Cars
            .AsNoTracking()
            .Where(car => ids.Contains(car.Id))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Car> Items, int TotalCount)> SearchAsync(
        string? search,
        string? make,
        string? model,
        string? fuelType,
        string? vehicleClass,
        string? transmission,
        int? yearFrom,
        int? yearTo,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Cars.AsNoTracking().AsQueryable();

        string? brandBoostToken = null;
        string? modelBoostToken = null;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var tokens = CarSearchText.Tokenize(search);

            // Every token must match somewhere across make / model / trim / class / fuel / year.
            // "alcazar" → model; "hyundai alcazar" / "hyundi alcazar" → make + model.
            foreach (var token in tokens)
            {
                var t = token;
                if (int.TryParse(t, out var yearToken) && yearToken is >= 1990 and <= 2100)
                {
                    query = query.Where(car =>
                        car.Year == yearToken ||
                        car.Make.ToLower().Contains(t) ||
                        car.Model.ToLower().Contains(t) ||
                        (car.Trim != null && car.Trim.ToLower().Contains(t)));
                }
                else
                {
                    query = query.Where(car =>
                        car.Make.ToLower().Contains(t) ||
                        car.Model.ToLower().Contains(t) ||
                        (car.Make.ToLower() + " " + car.Model.ToLower()).Contains(t) ||
                        (car.Trim != null && car.Trim.ToLower().Contains(t)) ||
                        car.VehicleClass.ToLower().Contains(t) ||
                        (car.FuelType != null && car.FuelType.ToLower().Contains(t)));
                }
            }

            if (tokens.Count >= 1)
            {
                brandBoostToken = tokens[0];
            }

            if (tokens.Count >= 2)
            {
                modelBoostToken = tokens[^1];
            }
            else if (tokens.Count == 1)
            {
                modelBoostToken = tokens[0];
            }
        }

        if (!string.IsNullOrWhiteSpace(make))
        {
            var makeTerm = make.Trim().ToLowerInvariant();
            query = query.Where(car => car.Make.ToLower() == makeTerm);
        }

        if (!string.IsNullOrWhiteSpace(model))
        {
            var modelTerm = model.Trim().ToLowerInvariant();
            query = query.Where(car => car.Model.ToLower() == modelTerm);
        }

        if (!string.IsNullOrWhiteSpace(fuelType))
        {
            var fuelTerm = fuelType.Trim().ToLowerInvariant();
            query = query.Where(car => car.FuelType != null && car.FuelType.ToLower().Contains(fuelTerm));
        }

        if (!string.IsNullOrWhiteSpace(vehicleClass))
        {
            var classTerm = vehicleClass.Trim().ToLowerInvariant();
            query = query.Where(car => car.VehicleClass.ToLower().Contains(classTerm));
        }

        if (!string.IsNullOrWhiteSpace(transmission))
        {
            var transmissionTerm = transmission.Trim().ToLowerInvariant();
            query = query.Where(car => car.Transmission != null && car.Transmission.ToLower().Contains(transmissionTerm));
        }

        if (yearFrom is not null)
        {
            query = query.Where(car => car.Year >= yearFrom);
        }

        if (yearTo is not null)
        {
            query = query.Where(car => car.Year <= yearTo);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Prefer rows where early tokens look like brand and later tokens like model.
        IOrderedQueryable<Car> ordered;
        if (!string.IsNullOrWhiteSpace(brandBoostToken) && !string.IsNullOrWhiteSpace(modelBoostToken) &&
            !string.Equals(brandBoostToken, modelBoostToken, StringComparison.Ordinal))
        {
            var brandToken = brandBoostToken;
            var modelToken = modelBoostToken;
            ordered = query
                .OrderByDescending(car => car.Make.ToLower().Contains(brandToken) ? 1 : 0)
                .ThenByDescending(car => car.Model.ToLower().Contains(modelToken) ? 1 : 0)
                .ThenByDescending(car => car.UpdatedAtUtc)
                .ThenBy(car => car.Make)
                .ThenBy(car => car.Model)
                .ThenBy(car => car.Year);
        }
        else if (!string.IsNullOrWhiteSpace(modelBoostToken))
        {
            var modelToken = modelBoostToken;
            ordered = query
                .OrderByDescending(car => car.Model.ToLower().Contains(modelToken) ? 2 :
                    car.Make.ToLower().Contains(modelToken) ? 1 : 0)
                .ThenByDescending(car => car.UpdatedAtUtc)
                .ThenBy(car => car.Make)
                .ThenBy(car => car.Model)
                .ThenBy(car => car.Year);
        }
        else
        {
            ordered = query
                .OrderByDescending(car => car.UpdatedAtUtc)
                .ThenBy(car => car.Make)
                .ThenBy(car => car.Model)
                .ThenBy(car => car.Year);
        }

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return 0;
        }

        var cars = await dbContext.Cars
            .Where(car => ids.Contains(car.Id))
            .ToListAsync(cancellationToken);
        if (cars.Count == 0)
        {
            return 0;
        }

        dbContext.Cars.RemoveRange(cars);
        return cars.Count;
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
