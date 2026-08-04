namespace Automantri.Application.Imports;

public sealed record CatalogImportPreviewRequest(
    string Brand = "Hyundai",
    int Year = 2024,
    bool SyncDeletes = true);

public sealed record CatalogImportRowDto(
    string ClientId,
    string Brand,
    string Model,
    int Year,
    string Variant,
    string? Transmission,
    string? FuelType,
    string? VehicleClass,
    string? Engines,
    IReadOnlyList<string> Features,
    string Action,
    Guid? ExistingId,
    string? SheetName);

public sealed record CatalogImportPreviewResultDto(
    string Brand,
    int Year,
    IReadOnlyList<string> Sheets,
    IReadOnlyList<CatalogImportRowDto> Rows,
    int CreateCount,
    int UpdateCount,
    int DeleteCount);

public sealed record CatalogImportCommitRequest(
    string Brand,
    int Year,
    bool SyncDeletes,
    IReadOnlyList<CatalogImportRowDto> Rows);

public sealed record CatalogImportCommitResultDto(
    int InsertedCount,
    int UpdatedCount,
    int DeletedCount,
    int SkippedCount,
    string Message);
