using System.Text.Json.Serialization;

namespace Automantri.Infrastructure.External.ApiNinjas;

internal sealed class ApiNinjasCarResponse
{
    [JsonPropertyName("city_mpg")]
    [JsonConverter(typeof(FlexibleIntJsonConverter))]
    public int CityMpg { get; set; }

    [JsonPropertyName("class")]
    public string? VehicleClass { get; set; }

    [JsonPropertyName("combination_mpg")]
    [JsonConverter(typeof(FlexibleIntJsonConverter))]
    public int CombinationMpg { get; set; }

    [JsonPropertyName("cylinders")]
    [JsonConverter(typeof(FlexibleNullableIntJsonConverter))]
    public int? Cylinders { get; set; }

    [JsonPropertyName("displacement")]
    [JsonConverter(typeof(FlexibleNullableDecimalJsonConverter))]
    public decimal? Displacement { get; set; }

    [JsonPropertyName("drive")]
    public string? Drive { get; set; }

    [JsonPropertyName("fuel_type")]
    public string? FuelType { get; set; }

    [JsonPropertyName("highway_mpg")]
    [JsonConverter(typeof(FlexibleIntJsonConverter))]
    public int HighwayMpg { get; set; }

    [JsonPropertyName("make")]
    public string? Make { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("transmission")]
    public string? Transmission { get; set; }

    [JsonPropertyName("year")]
    [JsonConverter(typeof(FlexibleIntJsonConverter))]
    public int Year { get; set; }
}
