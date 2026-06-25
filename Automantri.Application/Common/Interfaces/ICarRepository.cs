using Automantri.Domain.Entities;

namespace Automantri.Application.Common.Interfaces;

public interface ICarRepository
{
    Task<CarUpsertResult> UpsertRangeAsync(IReadOnlyCollection<Car> cars, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Car>> GetAllAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed record CarUpsertResult(int InsertedCount, int UpdatedCount);
