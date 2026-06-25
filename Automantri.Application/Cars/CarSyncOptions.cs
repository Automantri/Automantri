namespace Automantri.Application.Cars;

public sealed class CarSyncOptions
{
    public const string SectionName = "CarSync";

    public TimeSpan Interval { get; set; } = TimeSpan.FromDays(7);

    public bool Enabled { get; set; } = true;

    public bool RunOnStartup { get; set; } = false;

    public bool SyncFullCatalog { get; set; } = true;

    public int RequestDelayMilliseconds { get; set; } = 250;

    public int SaveBatchSize { get; set; } = 25;

    public List<CarSyncTarget> Targets { get; set; } = [];

    /// <summary>
    /// Used when /v1/carmakes and /v1/carmodels are unavailable on the current API Ninjas plan.
    /// </summary>
    public List<CatalogSeed> CatalogSeeds { get; set; } = [];
}

public sealed class CarSyncTarget
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}

public sealed class CatalogSeed
{
    public string Make { get; set; } = string.Empty;
    public List<string> Models { get; set; } = [];
}
