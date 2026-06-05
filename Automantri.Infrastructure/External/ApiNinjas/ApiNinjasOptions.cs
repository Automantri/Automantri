namespace Automantri.Infrastructure.External.ApiNinjas;

public sealed class ApiNinjasOptions
{
    public const string SectionName = "ApiNinjas";

    public string BaseUrl { get; set; } = "https://api.api-ninjas.com/v1/cars";
    public string ApiKey { get; set; } = string.Empty;
}
