namespace Automantri.Application.Cars;

public interface ICarSearchService
{
    Task<IReadOnlyCollection<CarDto>> ImportByModelAsync(string model, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CarDto>> GetSavedCarsAsync(CancellationToken cancellationToken);
}
