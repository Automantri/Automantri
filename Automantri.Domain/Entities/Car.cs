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
    public string SourceQuery { get; set; } = string.Empty;
    public DateTimeOffset RetrievedAtUtc { get; set; }
}
