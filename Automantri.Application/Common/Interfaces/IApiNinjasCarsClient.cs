using Automantri.Domain.Entities;

namespace Automantri.Application.Common.Interfaces;

public interface IApiNinjasCarsClient
{
    Task<IReadOnlyCollection<Car>> GetCarsByModelAsync(string model, CancellationToken cancellationToken);
}
