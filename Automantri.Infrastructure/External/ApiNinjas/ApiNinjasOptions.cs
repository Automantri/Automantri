namespace Automantri.Infrastructure.External.ApiNinjas;

public sealed class ApiNinjasOptions
{
    public const string SectionName = "ApiNinjas";

    public string ApiRootUrl { get; set; } = "https://api.api-ninjas.com/v1";
    public string ApiKey { get; set; } = string.Empty;

    public string CarsUrl => $"{ApiRootUrl.TrimEnd('/')}/cars";
    public string CarMakesUrl => $"{ApiRootUrl.TrimEnd('/')}/carmakes";
    public string CarModelsUrl => $"{ApiRootUrl.TrimEnd('/')}/carmodels";
    public string CarTrimsUrl => $"{ApiRootUrl.TrimEnd('/')}/cartrims";
    public string CarDetailsUrl => $"{ApiRootUrl.TrimEnd('/')}/cardetails";
}
