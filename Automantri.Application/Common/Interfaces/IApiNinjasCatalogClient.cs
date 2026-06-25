namespace Automantri.Application.Common.Interfaces;

public interface IApiNinjasCatalogClient
{
    Task<IReadOnlyCollection<string>> GetMakesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetModelsAsync(string make, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CarTrimDetails>> GetTrimDetailsAsync(
        string make,
        string model,
        CancellationToken cancellationToken);
}

public sealed record CarTrimDetails(
    string Make,
    string Model,
    string Trim,
    string? Generation,
    string? Serie,
    string? CarType,
    int? StartProductionYear,
    int? EndProductionYear,
    string SpecificationsJson);
