using System.Text.Json.Serialization;

namespace Automantri.Infrastructure.External.ApiNinjas;

internal sealed class ApiNinjasCarDetailsResponse
{
    [JsonPropertyName("make")]
    public string? Make { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("trim")]
    public string? Trim { get; set; }

    [JsonPropertyName("start_production_year")]
    public int? StartProductionYear { get; set; }

    [JsonPropertyName("end_production_year")]
    public int? EndProductionYear { get; set; }

    [JsonPropertyName("specifications")]
    public Dictionary<string, string>? Specifications { get; set; }
}
