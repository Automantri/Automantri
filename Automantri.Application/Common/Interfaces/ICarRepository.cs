using Automantri.Domain.Entities;

namespace Automantri.Application.Common.Interfaces;

public interface ICarRepository
{
    Task<CarUpsertResult> UpsertRangeAsync(IReadOnlyCollection<Car> cars, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Car>> GetAllAsync(CancellationToken cancellationToken);
    Task<Car?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Car>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<Car> Items, int TotalCount)> SearchAsync(
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
        CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record CarUpsertResult(int InsertedCount, int UpdatedCount);
