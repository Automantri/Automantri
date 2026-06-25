using Automantri.Application.Common.Interfaces;

namespace Automantri.Application.Cars;

public sealed class CarSearchService(ICarRepository carRepository) : ICarSearchService
{
    public async Task<IReadOnlyCollection<CarDto>> GetSavedCarsAsync(CancellationToken cancellationToken)
    {
        var cars = await carRepository.GetAllAsync(cancellationToken);
        return cars.Select(CarDto.FromEntity).ToArray();
    }
}
