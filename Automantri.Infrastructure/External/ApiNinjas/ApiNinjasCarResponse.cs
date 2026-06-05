using System.Text.Json.Serialization;

namespace Automantri.Infrastructure.External.ApiNinjas;

internal sealed class ApiNinjasCarResponse
{
    [JsonPropertyName("city_mpg")]
    public int CityMpg { get; set; }

    [JsonPropertyName("class")]
    public string? VehicleClass { get; set; }

    [JsonPropertyName("combination_mpg")]
    public int CombinationMpg { get; set; }

    [JsonPropertyName("cylinders")]
    public int? Cylinders { get; set; }

    [JsonPropertyName("displacement")]
    public decimal? Displacement { get; set; }

    [JsonPropertyName("drive")]
    public string? Drive { get; set; }

    [JsonPropertyName("fuel_type")]
    public string? FuelType { get; set; }

    [JsonPropertyName("highway_mpg")]
    public int HighwayMpg { get; set; }

    [JsonPropertyName("make")]
    public string? Make { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("transmission")]
    public string? Transmission { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }
}
