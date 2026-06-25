namespace Automantri.Domain.Entities;

public sealed class Car
{
    public Guid Id { get; set; }
    public int CityMpg { get; set; }
    public string VehicleClass { get; set; } = string.Empty;
    public int CombinationMpg { get; set; }
    public int? Cylinders { get; set; }
    public decimal? Displacement { get; set; }
    public string? Drive { get; set; }
    public string? FuelType { get; set; }
    public int HighwayMpg { get; set; }
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? Transmission { get; set; }
    public int Year { get; set; }
    public string? Trim { get; set; }
    public string? Generation { get; set; }
    public string? Serie { get; set; }
    public string? CarType { get; set; }
    public int? StartProductionYear { get; set; }
    public int? EndProductionYear { get; set; }
    public string? SpecificationsJson { get; set; }
    public string SourceQuery { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTimeOffset RetrievedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
