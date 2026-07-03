using Automantri.Application.Cars;
using Automantri.Application.Common.Interfaces;
using Automantri.Application.Frontend;
using Automantri.Infrastructure.Cars;
using Automantri.Infrastructure.External.ApiNinjas;
using Automantri.Infrastructure.External.CarImages;
using Automantri.Infrastructure.Persistence;
using Automantri.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Automantri.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApiNinjasOptions>(configuration.GetSection(ApiNinjasOptions.SectionName));
        services.Configure<CarSyncOptions>(configuration.GetSection(CarSyncOptions.SectionName));
        services.Configure<CarImagesOptions>(configuration.GetSection(CarImagesOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. Set ConnectionStrings:DefaultConnection in appsettings.json, appsettings.Development.json, user secrets, or an environment variable.");
        }

        services.AddDbContext<AutomantriDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpClient<IApiNinjasCarsClient, ApiNinjasCarsClient>();
        services.AddHttpClient<IApiNinjasCatalogClient, ApiNinjasCatalogClient>();
        services.AddSingleton<ICarImageResolver, CarImageResolver>();
        services.AddScoped<ICarRepository, CarRepository>();
        services.AddScoped<ICarSyncService, CarSyncService>();
        services.AddScoped<ICarSearchService, CarSearchService>();
        services.AddSingleton<ICarEnrichmentService, CarEnrichmentService>();
        services.AddScoped<IFrontendCarService, FrontendCarService>();
        services.AddSingleton<IContentService, ContentService>();
        services.AddHostedService<CarSyncBackgroundService>();

        return services;
    }
}
