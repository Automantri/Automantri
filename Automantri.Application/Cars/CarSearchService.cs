using Automantri.Application.Common.Interfaces;

namespace Automantri.Application.Cars;

public sealed class CarSearchService(
    IApiNinjasCarsClient carsClient,
    ICarRepository carRepository) : ICarSearchService
{
    public async Task<IReadOnlyCollection<CarDto>> ImportByModelAsync(string model, CancellationToken cancellationToken)
    {
        var cars = await carsClient.GetCarsByModelAsync(model, cancellationToken);

        if (cars.Count > 0)
        {
            await carRepository.AddRangeAsync(cars, cancellationToken);
            await carRepository.SaveChangesAsync(cancellationToken);
        }

        return cars.Select(CarDto.FromEntity).ToArray();
    }

    public async Task<IReadOnlyCollection<CarDto>> GetSavedCarsAsync(CancellationToken cancellationToken)
    {
        var cars = await carRepository.GetAllAsync(cancellationToken);
        return cars.Select(CarDto.FromEntity).ToArray();
    }
}
