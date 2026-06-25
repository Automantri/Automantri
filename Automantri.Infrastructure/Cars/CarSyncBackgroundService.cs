using Automantri.Application.Cars;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Automantri.Infrastructure.Cars;

internal sealed class CarSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<CarSyncOptions> syncOptions,
    ILogger<CarSyncBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!syncOptions.Value.Enabled)
        {
            logger.LogInformation("Car sync background service is disabled.");
            return;
        }

        if (syncOptions.Value.RunOnStartup)
        {
            await RunSyncAsync(stoppingToken);
        }

        var interval = syncOptions.Value.Interval;
        if (interval <= TimeSpan.Zero)
        {
            interval = TimeSpan.FromDays(7);
        }

        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunSyncAsync(stoppingToken);
        }
    }

    private async Task RunSyncAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<ICarSyncService>();

            if (syncOptions.Value.SyncFullCatalog)
            {
                var result = await syncService.SyncFullCatalogAsync(cancellationToken);
                logger.LogInformation(
                    "Full catalog sync completed. Makes={Makes}, Models={Models}, Inserted={Inserted}, Updated={Updated}, TrimDetails={TrimDetails}.",
                    result.MakesProcessed,
                    result.ModelsProcessed,
                    result.InsertedCount,
                    result.UpdatedCount,
                    result.TrimDetailsProcessed);
                return;
            }

            var results = await syncService.SyncAllConfiguredAsync(cancellationToken);
            foreach (var result in results)
            {
                logger.LogInformation(
                    "Synced {Make}/{Model}: {Inserted} inserted, {Updated} updated.",
                    result.Make,
                    result.Model,
                    result.InsertedCount,
                    result.UpdatedCount);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Scheduled car sync failed.");
        }
    }
}
