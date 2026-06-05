using System.Net.Http.Headers;
using System.Net.Http.Json;
using Automantri.Application.Common.Interfaces;
using Automantri.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Automantri.Infrastructure.External.ApiNinjas;

internal sealed class ApiNinjasCarsClient(
    HttpClient httpClient,
    IOptions<ApiNinjasOptions> options) : IApiNinjasCarsClient
{
    public async Task<IReadOnlyCollection<Car>> GetCarsByModelAsync(
        string model,
        CancellationToken cancellationToken)
    {
        var apiOptions = options.Value;
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{apiOptions.BaseUrl}?model={Uri.EscapeDataString(model)}");

        request.Headers.Add("X-Api-Key", apiOptions.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var cars = await response.Content.ReadFromJsonAsync<ApiNinjasCarResponse[]>(cancellationToken)
            ?? [];

        var retrievedAtUtc = DateTimeOffset.UtcNow;

        return cars.Select(car => new Car
        {
            Id = Guid.NewGuid(),
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
            Year = car.Year,
            SourceQuery = model,
            RetrievedAtUtc = retrievedAtUtc
        }).ToArray();
    }
}
