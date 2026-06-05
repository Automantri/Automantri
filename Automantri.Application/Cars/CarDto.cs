using Automantri.Domain.Entities;

namespace Automantri.Application.Cars;

public sealed record CarDto(
    Guid Id,
    int CityMpg,
    string VehicleClass,
    int CombinationMpg,
    int? Cylinders,
    decimal? Displacement,
    string? Drive,
    string? FuelType,
    int HighwayMpg,
    string Make,
    string Model,
    string? Transmission,
    int Year,
    string SourceQuery,
    DateTimeOffset RetrievedAtUtc)
{
    public static CarDto FromEntity(Car car)
    {
        return new CarDto(
            car.Id,
            car.CityMpg,
            car.VehicleClass,
            car.CombinationMpg,
            car.Cylinders,
            car.Displacement,
            car.Drive,
            car.FuelType,
            car.HighwayMpg,
            car.Make,
            car.Model,
            car.Transmission,
            car.Year,
            car.SourceQuery,
            car.RetrievedAtUtc);
    }
}
