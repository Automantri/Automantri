using System.Net.Http.Headers;
using System.Net.Http.Json;
using Automantri.Application.Common.Interfaces;
using Automantri.Domain.Entities;
using Automantri.Infrastructure.External.ApiNinjas;
using Microsoft.Extensions.Options;

namespace Automantri.Infrastructure.External.ApiNinjas;

internal sealed class ApiNinjasCarsClient(
    HttpClient httpClient,
    IOptions<ApiNinjasOptions> options) : IApiNinjasCarsClient
{
    public async Task<IReadOnlyCollection<Car>> GetCarsAsync(
        string make,
        string model,
        CancellationToken cancellationToken)
    {
        var apiOptions = options.Value;
        var query = $"?make={Uri.EscapeDataString(make)}&model={Uri.EscapeDataString(model)}";
        var cars = await ApiNinjasHttp.SendAsync<ApiNinjasCarResponse[]>(
            httpClient,
            apiOptions.ApiKey,
            $"{apiOptions.CarsUrl}{query}",
            cancellationToken) ?? [];

        return cars
            .Select(MapToEntity)
            .Where(car => car.Year > 0)
            .ToArray();
    }

    private static Car MapToEntity(ApiNinjasCarResponse car)
    {
        return new Car
        {
            CityMpg = car.CityMpg,
            VehicleClass = car.VehicleClass ?? string.Empty,
            CombinationMpg = car.CombinationMpg,
            Cylinders = car.Cylinders,
            Displacement = car.Displacement,
            Drive = car.Drive,
            FuelType = car.FuelType,
            HighwayMpg = car.HighwayMpg,
            Make = car.Make ?? string.Empty,
            Model = car.Model ?? string.Empty,
            Transmission = car.Transmission,
            Year = car.Year
        };
    }
}
