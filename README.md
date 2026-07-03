# Automantri API

ASP.NET Core 10 Web API for car catalog sync and frontend integration.

## Related frontend

The React frontend lives at `/home/nadmin/repo/NewCars`. See [INTEGRATION.md](../NewCars/INTEGRATION.md) in that repo for the full integration guide.

## Run locally

```bash
dotnet run --project Automantri.Api
```

- HTTP: `http://localhost:5230`
- Swagger: `http://localhost:5230/swagger` (Development only)
- Health: `http://localhost:5230/health`

## Sync data

```bash
curl -X POST "http://localhost:5230/api/cars/sync?make=toyota&model=camry"
# or full catalog:
curl -X POST "http://localhost:5230/api/cars/sync/catalog"
```

## API controllers

| Controller | Route prefix |
|------------|--------------|
| `CarsController` | `/api/cars` |
| `CatalogController` | `/api/catalog` |
| `TcoController` | `/api/tco` |
| `AdviceController` | `/api/advice` |
| `ContentController` | `/api/content` |
| `TestDrivesController` | `/api/test-drives` |

Full endpoint documentation: [NewCars/INTEGRATION.md](../NewCars/INTEGRATION.md)
