namespace Automantri.Application.Cars;

public interface ICarSearchService
{
    Task<IReadOnlyCollection<CarDto>> GetSavedCarsAsync(CancellationToken cancellationToken);
}
