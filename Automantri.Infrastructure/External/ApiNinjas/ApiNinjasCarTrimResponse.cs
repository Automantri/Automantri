using System.Text.Json.Serialization;

namespace Automantri.Infrastructure.External.ApiNinjas;

internal sealed class ApiNinjasCarTrimResponse
{
    [JsonPropertyName("make")]
    public string? Make { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("generation")]
    public string? Generation { get; set; }

    [JsonPropertyName("serie")]
    public string? Serie { get; set; }

    [JsonPropertyName("trim")]
    public string? Trim { get; set; }

    [JsonPropertyName("trim_start_production_year")]
    public int? TrimStartProductionYear { get; set; }

    [JsonPropertyName("trim_end_production_year")]
    public int? TrimEndProductionYear { get; set; }

    [JsonPropertyName("car_type")]
    public string? CarType { get; set; }
}
