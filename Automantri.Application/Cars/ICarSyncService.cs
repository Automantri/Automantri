namespace Automantri.Application.Cars;

public interface ICarSyncService
{
    Task<IReadOnlyCollection<string>> GetAvailableMakesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetAvailableModelsAsync(
        string make,
        CancellationToken cancellationToken);

    Task<CatalogSyncResultDto> SyncFullCatalogAsync(CancellationToken cancellationToken);

    Task<CarSyncResultDto> SyncByMakeModelAsync(
        string make,
        string model,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CarSyncResultDto>> SyncAllConfiguredAsync(
        CancellationToken cancellationToken);
}
