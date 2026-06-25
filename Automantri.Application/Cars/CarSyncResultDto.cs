namespace Automantri.Application.Cars;

public sealed record CarSyncResultDto(
    string Make,
    string Model,
    int InsertedCount,
    int UpdatedCount,
    IReadOnlyCollection<CarDto> Cars);
