namespace Automantri.Application.Frontend;

public sealed record CarListItemDto(
    string Id,
    Guid BackendId,
    string Brand,
    string Model,
    int Year,
    string Price,
    long PriceValue,
    double Rating,
    int Reviews,
    string Category,
    string FuelType,
    string Transmission,
    string Mileage,
    double MileageValue,
    int SafetyRating,
    IReadOnlyList<string> Features,
    string Image,
    bool Trending,
    bool Recommended);

public sealed record CarDetailDto(
    string Id,
    Guid BackendId,
    string Brand,
    string Model,
    string Variant,
    int Year,
    long Price,
    long OnRoad,
    string FuelType,
    string Transmission,
    double Mileage,
    string Color,
    IReadOnlyList<string> Images,
    string Location,
    double Rating,
    int Reviews,
    int VehicleScore,
    long TcoMonthly,
    int ResalePercent,
    string GenAiVerdict,
    string Engine,
    string Power,
    string Torque,
    long ServiceCostYearly,
    string Segment,
    int Seating,
    int BootSpace,
    int GroundClearance,
    int NcapRating,
    int Airbags,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<string> BestFor);

public sealed record CarVariantDto(
    string Name,
    long Price,
    string FuelType,
    string Transmission,
    IReadOnlyList<string> KeyFeatures,
    bool Recommended);

public sealed record CarReviewDto(
    string Id,
    string Author,
    string Location,
    string Date,
    double Rating,
    string Variant,
    string Kms,
    string Title,
    string Content,
    int Likes,
    bool Verified,
    string Sentiment,
    IReadOnlyDictionary<string, int> Categories);

public sealed record CarCompetitorDto(
    string Id,
    string Name,
    string Brand,
    long Price,
    double Mileage,
    string Power,
    double Rating,
    int Resale,
    long Tco,
    string Image);

public sealed record ComparisonCarDto(
    string Id,
    string Brand,
    string Model,
    string Image,
    string Price,
    double Rating,
    int Reviews,
    string Category,
    string FuelType,
    string Transmission,
    string Mileage,
    int SafetyRating,
    IReadOnlyList<string> Features,
    OwnerReviewDto OwnerReview,
    MarketTrendDto MarketTrend,
    OwnershipCostDto OwnershipCost,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<string> BestFor);

public sealed record OwnerReviewDto(
    string User,
    string Text,
    double Rating,
    string OwnershipPeriod);

public sealed record MarketTrendDto(
    string Demand,
    int Percentage,
    string? CityTrend);

public sealed record OwnershipCostDto(
    string Emi,
    string Insurance,
    string Service,
    string ResaleValue,
    string Total5Year);

public sealed record PopularComparisonDto(
    string Id,
    string Title,
    string Description,
    IReadOnlyList<string> CarIds);

public sealed record RecommendationDto(
    string Id,
    string Brand,
    string Model,
    string Image,
    string Price,
    double Rating,
    string Category,
    string Insight,
    string FuelType,
    string Transmission);

public sealed record TrendingItemDto(
    string Id,
    string Label,
    string Type,
    string Image);

public sealed record BrandDto(
    string Id,
    string Name,
    string Logo,
    string Country,
    IReadOnlyList<string> PopularModels);

public sealed record TcoRequestDto(
    string? CarId,
    Guid? BackendId,
    int MonthlyKm = 1500,
    int OwnershipYears = 5,
    double DownPaymentPercent = 20,
    double InterestRate = 8.5);

public sealed record TcoResultDto(
    string CarId,
    string Brand,
    string Model,
    long OnRoadPrice,
    long MonthlyEmi,
    long MonthlyFuel,
    long MonthlyInsurance,
    long MonthlyService,
    long MonthlyDepreciation,
    long TotalMonthlyCost,
    long TotalOwnershipCost,
    long NetCostAfterResale,
    int ResalePercent);

public sealed record SellOrKeepRequestDto(string CarDescription, int? CurrentKm, int? OwnershipMonths);

public sealed record SellOrKeepResultDto(
    string Recommendation,
    string EstimatedValueRange,
    string Reasoning,
    int ConfidencePercent);

public sealed record RepairAdviceRequestDto(string ProblemDescription, string? CarModel);

public sealed record RepairAdviceResultDto(
    string EstimatedCostRange,
    string Recommendation,
    string Urgency,
    IReadOnlyList<string> SuggestedActions);

public sealed record TestimonialDto(
    string Id,
    string Name,
    string Location,
    string Car,
    string Quote,
    double Rating,
    string Image,
    string Savings);

public sealed record ActivityItemDto(
    string Id,
    string User,
    string Action,
    string Car,
    string TimeAgo,
    string Location);

public sealed record PlatformStatsDto(
    int TotalUsers,
    double AverageRating,
    string TotalSavings,
    int CarsCompared);

public sealed record JourneyStepDto(
    string Id,
    int Order,
    string Phase,
    string Title,
    string Description,
    string CtaLink,
    string Icon);

public sealed record JourneySectionDto(
    string Eyebrow,
    string Title,
    string TitleHighlight,
    string Subtitle,
    string CtaTitle,
    string CtaSubtitle,
    string CtaLabel,
    string CtaLink,
    IReadOnlyList<string> TrustItems,
    IReadOnlyList<JourneyStepDto> Steps);

public sealed record TestDriveRequestDto(
    string CarId,
    string Name,
    string Email,
    string Phone,
    string PreferredDate,
    string PreferredTime,
    string DealerLocation);

public sealed record TestDriveResultDto(string Id, string Status, string Message);
