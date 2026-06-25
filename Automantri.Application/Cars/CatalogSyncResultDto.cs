namespace Automantri.Application.Cars;

public sealed record CatalogSyncResultDto(
    int MakesProcessed,
    int ModelsProcessed,
    int InsertedCount,
    int UpdatedCount,
    int TrimDetailsProcessed);
