using Automantri.Application.Cars;

namespace Automantri.Application.Frontend;

public interface IFrontendCarService
{
    Task<CarSearchResultDto> SearchAsync(CarSearchQuery query, CancellationToken cancellationToken);
    Task<CarDetailDto?> GetDetailByIdAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CarListItemDto>> GetListItemsAsync(CarSearchQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ComparisonCarDto>> CompareAsync(IReadOnlyCollection<string> ids, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CarVariantDto>> GetVariantsAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CarReviewDto>> GetReviewsAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CarCompetitorDto>> GetCompetitorsAsync(string id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<RecommendationDto>> GetRecommendationsAsync(int count, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TrendingItemDto>> GetTrendingAsync(int count, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PopularComparisonDto>> GetPopularComparisonsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BrandDto>> GetBrandsAsync(CancellationToken cancellationToken);
    Task<TcoResultDto?> CalculateTcoAsync(TcoRequestDto request, CancellationToken cancellationToken);
    Task<SellOrKeepResultDto> GetSellOrKeepAdviceAsync(SellOrKeepRequestDto request);
    Task<RepairAdviceResultDto> GetRepairAdviceAsync(RepairAdviceRequestDto request);
    Task<TestDriveResultDto> BookTestDriveAsync(TestDriveRequestDto request);
}
