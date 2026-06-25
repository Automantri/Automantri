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
    string? Trim,
    string? Generation,
    string? Serie,
    string? CarType,
    int? StartProductionYear,
    int? EndProductionYear,
    string? SpecificationsJson,
    string SourceQuery,
    string? ImageUrl,
    DateTimeOffset RetrievedAtUtc,
    DateTimeOffset UpdatedAtUtc)
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
            car.Trim,
            car.Generation,
            car.Serie,
            car.CarType,
            car.StartProductionYear,
            car.EndProductionYear,
            car.SpecificationsJson,
            car.SourceQuery,
            car.ImageUrl,
            car.RetrievedAtUtc,
            car.UpdatedAtUtc);
    }
}
