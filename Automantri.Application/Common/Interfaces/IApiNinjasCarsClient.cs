using Automantri.Domain.Entities;

namespace Automantri.Application.Common.Interfaces;

public interface IApiNinjasCarsClient
{
    Task<IReadOnlyCollection<Car>> GetCarsAsync(
        string make,
        string model,
        CancellationToken cancellationToken);
}
