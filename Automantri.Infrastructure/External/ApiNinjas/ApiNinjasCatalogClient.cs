using System.Text.Json;
using Automantri.Application.Cars;
using Automantri.Application.Common.Interfaces;
using Automantri.Infrastructure.External.ApiNinjas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Automantri.Infrastructure.External.ApiNinjas;

internal sealed class ApiNinjasCatalogClient(
    HttpClient httpClient,
    IOptions<ApiNinjasOptions> apiOptions,
    IOptions<CarSyncOptions> syncOptions,
    ILogger<ApiNinjasCatalogClient> logger) : IApiNinjasCatalogClient
{
    public async Task<IReadOnlyCollection<string>> GetMakesAsync(CancellationToken cancellationToken)
    {
        var response = await ApiNinjasHttp.TrySendAsync<string[]>(
            httpClient,
            apiOptions.Value.ApiKey,
            apiOptions.Value.CarMakesUrl,
            cancellationToken);

        if (response.IsSuccess)
        {
            return NormalizeStringArray(response.Data);
        }

        logger.LogWarning(
            "API Ninjas /carmakes is unavailable ({StatusCode}). Falling back to CarSync:CatalogSeeds. Response: {Response}",
            (int)response.StatusCode,
            response.ErrorBody);

        return GetSeededMakes();
    }

    public async Task<IReadOnlyCollection<string>> GetModelsAsync(string make, CancellationToken cancellationToken)
    {
        var normalizedMake = make.Trim().ToLowerInvariant();
        var url = $"{apiOptions.Value.CarModelsUrl}?make={Uri.EscapeDataString(normalizedMake)}";
        var response = await ApiNinjasHttp.TrySendAsync<string[]>(
            httpClient,
            apiOptions.Value.ApiKey,
            url,
            cancellationToken);

        if (response.IsSuccess)
        {
            return NormalizeStringArray(response.Data);
        }

        logger.LogWarning(
            "API Ninjas /carmodels is unavailable for {Make} ({StatusCode}). Falling back to CarSync:CatalogSeeds. Response: {Response}",
            normalizedMake,
            (int)response.StatusCode,
            response.ErrorBody);

        return GetSeededModels(normalizedMake);
    }

    public async Task<IReadOnlyCollection<CarTrimDetails>> GetTrimDetailsAsync(
        string make,
        string model,
        CancellationToken cancellationToken)
    {
        var trims = await GetTrimsAsync(make, model, cancellationToken);
        if (trims.Count == 0)
        {
            return [];
        }

        var details = new List<CarTrimDetails>(trims.Count);
        foreach (var trim in trims)
        {
            if (string.IsNullOrWhiteSpace(trim.Trim))
            {
                continue;
            }

            var trimDetails = await GetCarDetailsAsync(make, model, trim.Trim, cancellationToken);
            foreach (var detail in trimDetails)
            {
                details.Add(MapTrimDetails(trim, detail));
            }
        }

        return details;
    }

    private async Task<IReadOnlyCollection<ApiNinjasCarTrimResponse>> GetTrimsAsync(
        string make,
        string model,
        CancellationToken cancellationToken)
    {
        var url =
            $"{apiOptions.Value.CarTrimsUrl}?make={Uri.EscapeDataString(make)}&model={Uri.EscapeDataString(model)}&limit=100";
        var response = await ApiNinjasHttp.TrySendAsync<ApiNinjasCarTrimResponse[]>(
            httpClient,
            apiOptions.Value.ApiKey,
            url,
            cancellationToken);

        if (response.IsSuccess)
        {
            return response.Data ?? [];
        }

        logger.LogDebug(
            "Car trims are unavailable for {Make}/{Model} ({StatusCode}).",
            make,
            model,
            (int)response.StatusCode);
        return [];
    }

    private async Task<IReadOnlyCollection<ApiNinjasCarDetailsResponse>> GetCarDetailsAsync(
        string make,
        string model,
        string trim,
        CancellationToken cancellationToken)
    {
        var url =
            $"{apiOptions.Value.CarDetailsUrl}?make={Uri.EscapeDataString(make)}&model={Uri.EscapeDataString(model)}&trim={Uri.EscapeDataString(trim)}";
        var response = await ApiNinjasHttp.TrySendAsync<ApiNinjasCarDetailsResponse[]>(
            httpClient,
            apiOptions.Value.ApiKey,
            url,
            cancellationToken);

        if (response.IsSuccess)
        {
            return response.Data ?? [];
        }

        logger.LogDebug(
            "Car details are unavailable for {Make}/{Model}/{Trim} ({StatusCode}).",
            make,
            model,
            trim,
            (int)response.StatusCode);
        return [];
    }

    private IReadOnlyCollection<string> GetSeededMakes()
    {
        return syncOptions.Value.CatalogSeeds
            .Where(seed => !string.IsNullOrWhiteSpace(seed.Make))
            .Select(seed => seed.Make.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(make => make, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private IReadOnlyCollection<string> GetSeededModels(string make)
    {
        return syncOptions.Value.CatalogSeeds
            .Where(seed => string.Equals(seed.Make.Trim(), make, StringComparison.OrdinalIgnoreCase))
            .SelectMany(seed => seed.Models)
            .Where(model => !string.IsNullOrWhiteSpace(model))
            .Select(model => model.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(model => model, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyCollection<string> NormalizeStringArray(string[]? values)
    {
        return values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
    }

    private static CarTrimDetails MapTrimDetails(
        ApiNinjasCarTrimResponse trim,
        ApiNinjasCarDetailsResponse detail)
    {
        var specificationsJson = detail.Specifications is null
            ? null
            : JsonSerializer.Serialize(detail.Specifications);

        return new CarTrimDetails(
            detail.Make ?? trim.Make ?? string.Empty,
            detail.Model ?? trim.Model ?? string.Empty,
            detail.Trim ?? trim.Trim ?? string.Empty,
            trim.Generation,
            trim.Serie,
            trim.CarType,
            detail.StartProductionYear ?? trim.TrimStartProductionYear,
            detail.EndProductionYear ?? trim.TrimEndProductionYear,
            specificationsJson ?? "{}");
    }
}
