using Automantri.Application.Cars;
using Automantri.Application.Common.Interfaces;
using Automantri.Domain.Entities;

namespace Automantri.Application.Frontend;

public sealed class FrontendCarService(
    ICarRepository carRepository,
    ICarEnrichmentService enrichmentService) : IFrontendCarService
{
    public async Task<CarSearchResultDto> SearchAsync(CarSearchQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await carRepository.SearchAsync(
            query.Search,
            query.Make,
            query.Model,
            query.FuelType,
            query.VehicleClass,
            query.Transmission,
            query.YearFrom,
            query.YearTo,
            query.SafePage,
            query.SafePageSize,
            cancellationToken);

        var dtos = items.Select(CarDto.FromEntity).Select(enrichmentService.ToListItem).ToArray();
        return new CarSearchResultDto(dtos, totalCount, query.SafePage, query.SafePageSize);
    }

    public async Task<CarDetailDto?> GetDetailByIdAsync(string id, CancellationToken cancellationToken)
    {
        var car = await ResolveCarAsync(id, cancellationToken);
        return car is null ? null : enrichmentService.ToDetail(CarDto.FromEntity(car));
    }

    public async Task<IReadOnlyCollection<CarListItemDto>> GetListItemsAsync(
        CarSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await SearchAsync(query, cancellationToken);
        return result.Items;
    }

    public async Task<IReadOnlyCollection<ComparisonCarDto>> CompareAsync(
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken)
    {
        var cars = await ResolveCarsAsync(ids, cancellationToken);
        return cars.Select(CarDto.FromEntity).Select(enrichmentService.ToComparison).ToArray();
    }

    public async Task<IReadOnlyCollection<CarVariantDto>> GetVariantsAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var car = await ResolveCarAsync(id, cancellationToken);
        if (car is null)
        {
            return [];
        }

        var allCars = await carRepository.GetAllAsync(cancellationToken);
        var variants = allCars
            .Where(c => c.Make.Equals(car.Make, StringComparison.OrdinalIgnoreCase) &&
                        c.Model.Equals(car.Model, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(c => c.Year)
            .ThenBy(c => c.Trim)
            .Select(CarDto.FromEntity)
            .ToArray();

        if (variants.Length == 0)
        {
            return [enrichmentService.ToVariant(CarDto.FromEntity(car), true)];
        }

        return variants
            .Select((variant, index) => enrichmentService.ToVariant(variant, index == 0))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<CarReviewDto>> GetReviewsAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var car = await ResolveCarAsync(id, cancellationToken);
        if (car is null)
        {
            return [];
        }

        var dto = CarDto.FromEntity(car);
        return Enumerable.Range(0, 3).Select(i => enrichmentService.ToReview(dto, i)).ToArray();
    }

    public async Task<IReadOnlyCollection<CarCompetitorDto>> GetCompetitorsAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var car = await ResolveCarAsync(id, cancellationToken);
        if (car is null)
        {
            return [];
        }

        var allCars = await carRepository.GetAllAsync(cancellationToken);
        var competitors = allCars
            .Where(c => c.Id != car.Id &&
                        c.VehicleClass.Equals(car.VehicleClass, StringComparison.OrdinalIgnoreCase))
            .GroupBy(c => $"{c.Make}/{c.Model}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(c => c.Year).First())
            .Take(3)
            .Select(CarDto.FromEntity)
            .Select(enrichmentService.ToCompetitor)
            .ToArray();

        return competitors;
    }

    public async Task<IReadOnlyCollection<RecommendationDto>> GetRecommendationsAsync(
        int count,
        CancellationToken cancellationToken)
    {
        var allCars = await carRepository.GetAllAsync(cancellationToken);
        var insights = new[]
        {
            "Best value in segment",
            "Low running costs",
            "Strong resale potential",
            "Feature-rich daily driver",
            "Great for city commuting",
            "Family-friendly choice",
            "Efficient highway cruiser"
        };

        var recommendations = allCars
            .GroupBy(c => $"{c.Make}/{c.Model}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(c => c.Year).First())
            .OrderByDescending(c => c.UpdatedAtUtc)
            .Take(Math.Clamp(count, 1, 20))
            .Select((car, index) =>
                enrichmentService.ToRecommendation(
                    CarDto.FromEntity(car),
                    insights[index % insights.Length]))
            .ToArray();

        return recommendations;
    }

    public async Task<IReadOnlyCollection<TrendingItemDto>> GetTrendingAsync(
        int count,
        CancellationToken cancellationToken)
    {
        var allCars = await carRepository.GetAllAsync(cancellationToken);
        return allCars
            .GroupBy(c => $"{c.Make}/{c.Model}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderByDescending(c => c.Year).First())
            .OrderByDescending(c => c.UpdatedAtUtc)
            .Take(Math.Clamp(count, 1, 20))
            .Select(car =>
            {
                var slug = CarEnrichmentService.BuildSlug(car);
                return new TrendingItemDto(
                    slug,
                    $"{char.ToUpperInvariant(car.Make[0]) + car.Make[1..]} {char.ToUpperInvariant(car.Model[0]) + car.Model[1..]}",
                    "car",
                    car.ImageUrl ?? string.Empty);
            })
            .ToArray();
    }

    public async Task<IReadOnlyCollection<PopularComparisonDto>> GetPopularComparisonsAsync(
        CancellationToken cancellationToken)
    {
        var recommendations = (await GetRecommendationsAsync(8, cancellationToken)).ToArray();
        if (recommendations.Length < 2)
        {
            return [];
        }

        var comparisons = new List<PopularComparisonDto>();
        for (var i = 0; i + 1 < recommendations.Length && comparisons.Count < 4; i += 2)
        {
            var first = recommendations[i];
            var second = recommendations[i + 1];
            comparisons.Add(new PopularComparisonDto(
                $"popular-{i / 2 + 1}",
                $"{first.Model} vs {second.Model}",
                $"Compare {first.Brand} {first.Model} and {second.Brand} {second.Model}",
                [first.Id, second.Id]));
        }

        return comparisons;
    }

    public async Task<IReadOnlyCollection<BrandDto>> GetBrandsAsync(CancellationToken cancellationToken)
    {
        var allCars = await carRepository.GetAllAsync(cancellationToken);
        return allCars
            .GroupBy(c => c.Make, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var make = g.Key;
                var title = char.ToUpperInvariant(make[0]) + make[1..];
                return new BrandDto(
                    make.ToLowerInvariant(),
                    title,
                    $"https://logo.clearbit.com/{make.ToLowerInvariant()}.com",
                    "Global",
                    g.Select(c => c.Model)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(5)
                        .Select(m => char.ToUpperInvariant(m[0]) + m[1..])
                        .ToArray());
            })
            .OrderBy(b => b.Name)
            .ToArray();
    }

    public async Task<TcoResultDto?> CalculateTcoAsync(
        TcoRequestDto request,
        CancellationToken cancellationToken)
    {
        Car? car = null;
        if (request.BackendId is not null)
        {
            car = await carRepository.GetByIdAsync(request.BackendId.Value, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.CarId))
        {
            car = await ResolveCarAsync(request.CarId, cancellationToken);
        }

        return car is null ? null : enrichmentService.CalculateTco(CarDto.FromEntity(car), request);
    }

    public Task<SellOrKeepResultDto> GetSellOrKeepAdviceAsync(SellOrKeepRequestDto request)
    {
        var keep = (request.OwnershipMonths ?? 12) < 36;
        return Task.FromResult(new SellOrKeepResultDto(
            keep ? "Keep" : "Sell",
            "₹4.8L – ₹5.2L",
            keep
                ? "Ownership cost is still favorable and resale remains strong for the next 1–2 years."
                : "Depreciation and maintenance costs suggest upgrading may be more economical now.",
            78));
    }

    public Task<RepairAdviceResultDto> GetRepairAdviceAsync(RepairAdviceRequestDto request)
    {
        var problem = request.ProblemDescription.ToLowerInvariant();
        var urgency = problem.Contains("brake") || problem.Contains("engine") ? "High" : "Medium";
        return Task.FromResult(new RepairAdviceResultDto(
            "₹3,500 – ₹12,000",
            urgency == "High"
                ? "Get inspected within 48 hours to avoid further damage."
                : "Schedule service in the next 1–2 weeks.",
            urgency,
            [
                "Book a certified service center inspection",
                "Compare OEM vs aftermarket part pricing",
                "Check warranty coverage before authorizing repairs"
            ]));
    }

    public Task<TestDriveResultDto> BookTestDriveAsync(TestDriveRequestDto request) =>
        Task.FromResult(new TestDriveResultDto(
            Guid.NewGuid().ToString("N")[..8],
            "pending",
            $"Test drive request received for {request.PreferredDate} at {request.DealerLocation}."));

    private async Task<Car?> ResolveCarAsync(string id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(id, out var guid))
        {
            return await carRepository.GetByIdAsync(guid, cancellationToken);
        }

        var allCars = await carRepository.GetAllAsync(cancellationToken);
        return allCars.FirstOrDefault(car =>
            CarEnrichmentService.BuildSlug(car).Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<IReadOnlyCollection<Car>> ResolveCarsAsync(
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var guidIds = ids.Where(id => Guid.TryParse(id, out _)).Select(Guid.Parse).ToArray();
        var cars = new List<Car>();

        if (guidIds.Length > 0)
        {
            cars.AddRange(await carRepository.GetByIdsAsync(guidIds, cancellationToken));
        }

        var slugIds = ids.Where(id => !Guid.TryParse(id, out _)).ToArray();
        if (slugIds.Length > 0)
        {
            var allCars = await carRepository.GetAllAsync(cancellationToken);
            foreach (var slug in slugIds)
            {
                var match = allCars.FirstOrDefault(car =>
                    CarEnrichmentService.BuildSlug(car).Equals(slug, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                {
                    cars.Add(match);
                }
            }
        }

        return cars
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToArray();
    }
}
