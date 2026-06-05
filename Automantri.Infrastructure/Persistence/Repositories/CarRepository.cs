using Automantri.Application.Common.Interfaces;
using Automantri.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Automantri.Infrastructure.Persistence.Repositories;

internal sealed class CarRepository(AutomantriDbContext dbContext) : ICarRepository
{
    public async Task AddRangeAsync(IReadOnlyCollection<Car> cars, CancellationToken cancellationToken)
    {
        await dbContext.Cars.AddRangeAsync(cars, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Car>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Cars
            .AsNoTracking()
            .OrderByDescending(car => car.RetrievedAtUtc)
            .ThenBy(car => car.Make)
            .ThenBy(car => car.Model)
            .ToArrayAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
