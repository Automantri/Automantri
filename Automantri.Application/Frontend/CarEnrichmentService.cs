using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Automantri.Application.Cars;
using Automantri.Domain.Entities;

namespace Automantri.Application.Frontend;

public interface ICarEnrichmentService
{
    CarListItemDto ToListItem(CarDto car);
    CarDetailDto ToDetail(CarDto car);
    ComparisonCarDto ToComparison(CarDto car);
    CarVariantDto ToVariant(CarDto car, bool recommended);
    CarReviewDto ToReview(CarDto car, int index);
    CarCompetitorDto ToCompetitor(CarDto car);
    RecommendationDto ToRecommendation(CarDto car, string insight);
    TcoResultDto CalculateTco(CarDto car, TcoRequestDto request);
}

public sealed class CarEnrichmentService : ICarEnrichmentService
{
    public CarListItemDto ToListItem(CarDto car)
    {
        var price = EstimatePrice(car);
        var mileage = EstimateMileage(car);
        var rating = DeterministicRating(car.Id, 4.0, 4.9);
        var reviews = DeterministicInt(car.Id, "reviews", 80, 1500);
        var features = ExtractFeatures(car);

        return new CarListItemDto(
            BuildSlug(car),
            car.Id,
            TitleCase(car.Make),
            TitleCase(car.Model),
            car.Year,
            FormatInr(price),
            price,
            rating,
            reviews,
            MapCategory(car),
            MapFuelType(car.FuelType),
            MapTransmission(car.Transmission),
            FormatMileage(car, mileage),
            mileage,
            DeterministicInt(car.Id, "safety", 4, 5),
            features,
            car.ImageUrl ?? string.Empty,
            DeterministicInt(car.Id, "trend", 0, 10) > 6,
            DeterministicInt(car.Id, "rec", 0, 10) > 5);
    }

    public CarDetailDto ToDetail(CarDto car)
    {
        var list = ToListItem(car);
        var onRoad = (long)(list.PriceValue * 1.12);
        var resale = DeterministicInt(car.Id, "resale", 58, 82);
        var tcoMonthly = CalculateTco(car, new TcoRequestDto(BuildSlug(car), car.Id)).TotalMonthlyCost;
        var engine = BuildEngineDescription(car);
        var power = BuildPowerDescription(car);

        return new CarDetailDto(
            list.Id,
            car.Id,
            list.Brand,
            list.Model,
            car.Trim ?? $"{list.Model} Base",
            car.Year,
            list.PriceValue,
            onRoad,
            list.FuelType,
            list.Transmission,
            list.MileageValue,
            "Standard",
            string.IsNullOrWhiteSpace(car.ImageUrl) ? [] : [car.ImageUrl],
            "Pan India",
            list.Rating,
            list.Reviews,
            DeterministicInt(car.Id, "score", 72, 96),
            tcoMonthly,
            resale,
            BuildVerdict(car, list),
            engine,
            power,
            BuildTorque(car),
            DeterministicInt(car.Id, "service", 8000, 18000),
            list.Category,
            MapSeating(car.VehicleClass),
            DeterministicInt(car.Id, "boot", 320, 580),
            DeterministicInt(car.Id, "gc", 165, 210),
            DeterministicInt(car.Id, "ncap", 3, 5),
            DeterministicInt(car.Id, "airbags", 4, 8),
            BuildStrengths(car),
            BuildWeaknesses(car),
            BuildBestFor(car));
    }

    public ComparisonCarDto ToComparison(CarDto car)
    {
        var list = ToListItem(car);
        var tco = CalculateTco(car, new TcoRequestDto(list.Id, car.Id));
        var ownerNames = new[] { "Priya S.", "Rajesh K.", "Amit M.", "Sneha R.", "Vikram P." };
        var ownerIndex = DeterministicInt(car.Id, "owner", 0, ownerNames.Length - 1);

        return new ComparisonCarDto(
            list.Id,
            list.Brand,
            list.Model,
            list.Image,
            list.Price,
            list.Rating,
            list.Reviews,
            list.Category,
            list.FuelType,
            list.Transmission,
            list.Mileage,
            list.SafetyRating,
            list.Features,
            new OwnerReviewDto(
                ownerNames[ownerIndex],
                $"After {DeterministicInt(car.Id, "months", 6, 24)} months, the {list.Model} delivers solid value for daily use.",
                Math.Round(list.Rating, 1),
                $"{DeterministicInt(car.Id, "months", 6, 24)} months"),
            new MarketTrendDto(
                "up",
                DeterministicInt(car.Id, "demand", 12, 45),
                $"Growing interest in {list.Brand} {list.Model}"),
            new OwnershipCostDto(
                FormatInr(tco.MonthlyEmi) + "/month",
                FormatInr(tco.MonthlyInsurance * 12) + "/year",
                FormatInr(tco.MonthlyService * 12) + "/year",
                $"{tco.ResalePercent}% after 3 years",
                FormatInr(tco.TotalOwnershipCost)),
            BuildStrengths(car),
            BuildWeaknesses(car),
            BuildBestFor(car));
    }

    public CarVariantDto ToVariant(CarDto car, bool recommended) =>
        new(
            car.Trim ?? $"{TitleCase(car.Model)} {car.Year}",
            EstimatePrice(car),
            MapFuelType(car.FuelType),
            MapTransmission(car.Transmission),
            ExtractFeatures(car),
            recommended);

    public CarReviewDto ToReview(CarDto car, int index)
    {
        var authors = new[] { "Arjun M.", "Deepa K.", "Rohan S.", "Meera P.", "Karan V." };
        var locations = new[] { "Mumbai", "Bangalore", "Delhi", "Hyderabad", "Pune" };
        var hash = Hash($"{car.Id}:{index}");
        var author = authors[hash % authors.Length];
        var location = locations[(hash / 7) % locations.Length];
        var rating = DeterministicRating(car.Id, 3.8, 5.0) + (index % 3) * 0.1;
        rating = Math.Min(5.0, Math.Round(rating, 1));

        return new CarReviewDto(
            $"{car.Id:N}-{index}",
            author,
            location,
            DateTime.UtcNow.AddDays(-(index + 1) * 37).ToString("MMM yyyy"),
            rating,
            car.Trim ?? "Base",
            $"{DeterministicInt(car.Id, $"kms-{index}", 8, 45)}k km",
            $"Great ownership experience with {TitleCase(car.Make)} {TitleCase(car.Model)}",
            $"The {TitleCase(car.Model)} offers a balanced mix of comfort, efficiency, and features for Indian roads.",
            DeterministicInt(car.Id, $"likes-{index}", 4, 120),
            index % 2 == 0,
            rating >= 4.2 ? "positive" : rating >= 3.5 ? "neutral" : "negative",
            new Dictionary<string, int>
            {
                ["comfort"] = DeterministicInt(car.Id, $"c-{index}", 3, 5),
                ["performance"] = DeterministicInt(car.Id, $"p-{index}", 3, 5),
                ["mileage"] = DeterministicInt(car.Id, $"m-{index}", 3, 5),
                ["features"] = DeterministicInt(car.Id, $"f-{index}", 3, 5),
            });
    }

    public CarCompetitorDto ToCompetitor(CarDto car)
    {
        var list = ToListItem(car);
        var tco = CalculateTco(car, new TcoRequestDto(list.Id, car.Id));

        return new CarCompetitorDto(
            list.Id,
            list.Model,
            list.Brand,
            list.PriceValue,
            list.MileageValue,
            BuildPowerDescription(car),
            list.Rating,
            tco.ResalePercent,
            tco.TotalMonthlyCost,
            list.Image);
    }

    public RecommendationDto ToRecommendation(CarDto car, string insight)
    {
        var list = ToListItem(car);
        return new RecommendationDto(
            list.Id,
            list.Brand,
            list.Model,
            list.Image,
            list.Price,
            list.Rating,
            list.Category,
            insight,
            list.FuelType,
            list.Transmission);
    }

    public TcoResultDto CalculateTco(CarDto car, TcoRequestDto request)
    {
        var onRoad = (long)(EstimatePrice(car) * 1.12);
        var loanPrincipal = (long)(onRoad * (1 - request.DownPaymentPercent / 100.0));
        var monthlyRate = request.InterestRate / 100 / 12;
        var months = request.OwnershipYears * 12;
        var emi = monthlyRate == 0
            ? loanPrincipal / months
            : (long)(loanPrincipal * monthlyRate * Math.Pow(1 + monthlyRate, months) /
                     (Math.Pow(1 + monthlyRate, months) - 1));

        var mileage = EstimateMileage(car);
        var fuelPrice = MapFuelType(car.FuelType) == "Electric" ? 8.0 : 105.0;
        var monthlyFuel = MapFuelType(car.FuelType) == "Electric"
            ? (long)(request.MonthlyKm * fuelPrice)
            : mileage <= 0 ? 9000 : (long)(request.MonthlyKm / mileage * fuelPrice);

        var monthlyInsurance = (long)(onRoad * 0.028 / 12);
        var monthlyService = DeterministicInt(car.Id, "service-month", 600, 1500);
        var resalePercent = DeterministicInt(car.Id, "resale", 58, 82);
        var monthlyDepreciation = (long)(onRoad * (1 - resalePercent / 100.0) / months);
        var totalMonthly = emi + monthlyFuel + monthlyInsurance + monthlyService + monthlyDepreciation;
        var totalOwnership = totalMonthly * months;
        var netAfterResale = totalOwnership - (long)(onRoad * resalePercent / 100.0);

        return new TcoResultDto(
            BuildSlug(car),
            TitleCase(car.Make),
            TitleCase(car.Model),
            onRoad,
            emi,
            monthlyFuel,
            monthlyInsurance,
            monthlyService,
            monthlyDepreciation,
            totalMonthly,
            totalOwnership,
            netAfterResale,
            resalePercent);
    }

    public static string BuildSlug(CarDto car) =>
        $"{car.Make}-{car.Model}-{car.Year}".ToLowerInvariant().Replace(' ', '-');

    public static string BuildSlug(Car car) =>
        $"{car.Make}-{car.Model}-{car.Year}".ToLowerInvariant().Replace(' ', '-');

    private static long EstimatePrice(CarDto car)
    {
        var baseByClass = car.VehicleClass.ToLowerInvariant() switch
        {
            var c when c.Contains("suv") => 14_50_000L,
            var c when c.Contains("truck") || c.Contains("pickup") => 18_00_000L,
            var c when c.Contains("van") || c.Contains("minivan") => 16_00_000L,
            var c when c.Contains("coupe") => 22_00_000L,
            _ => 11_50_000L
        };

        var yearFactor = 1.0 + Math.Clamp(car.Year - 2018, 0, 8) * 0.03;
        var trimFactor = 1.0 + DeterministicInt(car.Id, "trim-price", 0, 15) / 100.0;
        return (long)(baseByClass * yearFactor * trimFactor);
    }

    private static double EstimateMileage(CarDto car)
    {
        if (MapFuelType(car.FuelType) == "Electric")
        {
            return DeterministicInt(car.Id, "ev-range", 320, 480);
        }

        var mpg = car.CombinationMpg > 0 ? car.CombinationMpg : Math.Max(car.CityMpg, car.HighwayMpg);
        return Math.Round(mpg * 0.425, 1);
    }

    private static string FormatMileage(CarDto car, double mileage) =>
        MapFuelType(car.FuelType) == "Electric"
            ? $"{mileage:0} km/charge"
            : $"{mileage:0.#} kmpl";

    private static string FormatInr(long amount)
    {
        if (amount >= 100_00_000)
        {
            return $"₹{(amount / 100_00_000.0):0.##} Cr";
        }

        return $"₹{(amount / 100_000.0):0.##} Lakh";
    }

    private static string MapCategory(CarDto car)
    {
        var cls = car.VehicleClass.ToLowerInvariant();
        if (cls.Contains("suv")) return "SUV";
        if (cls.Contains("truck") || cls.Contains("pickup")) return "Pickup";
        if (cls.Contains("van") || cls.Contains("minivan")) return "MPV";
        if (cls.Contains("coupe")) return "Coupe";
        return "Sedan";
    }

    private static string MapFuelType(string? fuelType)
    {
        var fuel = (fuelType ?? string.Empty).ToLowerInvariant();
        if (fuel.Contains("electric")) return "Electric";
        if (fuel.Contains("diesel")) return "Diesel";
        if (fuel.Contains("hybrid")) return "Hybrid";
        return "Petrol";
    }

    private static string MapTransmission(string? transmission)
    {
        var value = (transmission ?? string.Empty).ToLowerInvariant();
        if (value.Contains("cvt")) return "CVT";
        if (value.Contains("auto")) return "Automatic";
        return "Manual";
    }

    private static int MapSeating(string vehicleClass)
    {
        var cls = vehicleClass.ToLowerInvariant();
        return cls.Contains("van") || cls.Contains("minivan") ? 7 : 5;
    }

    private static IReadOnlyList<string> ExtractFeatures(CarDto car)
    {
        if (!string.IsNullOrWhiteSpace(car.SpecificationsJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(car.SpecificationsJson);
                var features = doc.RootElement.EnumerateObject()
                    .Take(6)
                    .Select(p => $"{TitleCase(p.Name.Replace('_', ' '))}: {p.Value}")
                    .ToArray();
                if (features.Length > 0)
                {
                    return features;
                }
            }
            catch (JsonException)
            {
                // Fall through to defaults.
            }
        }

        return
        [
            $"{MapFuelType(car.FuelType)} Engine",
            MapTransmission(car.Transmission),
            $"{car.Year} Model Year",
            car.Drive is not null ? $"{car.Drive.ToUpperInvariant()} Drive" : "FWD Drive",
            $"{MapCategory(car)} Body Style",
            "Connected Car Tech"
        ];
    }

    private static string BuildEngineDescription(CarDto car)
    {
        if (car.Displacement is > 0)
        {
            return $"{car.Displacement:0.#}L {MapFuelType(car.FuelType)}";
        }

        return MapFuelType(car.FuelType) == "Electric"
            ? "Electric Motor"
            : $"{car.Cylinders ?? 4}-Cylinder {MapFuelType(car.FuelType)}";
    }

    private static string BuildPowerDescription(CarDto car)
    {
        var baseHp = car.Displacement is > 0 ? (int)(car.Displacement.Value * 55) : 120;
        baseHp += DeterministicInt(car.Id, "hp", 0, 40);
        return $"{baseHp} bhp";
    }

    private static string BuildTorque(CarDto car)
    {
        var baseNm = car.Displacement is > 0 ? (int)(car.Displacement.Value * 95) : 180;
        baseNm += DeterministicInt(car.Id, "torque", 0, 60);
        return $"{baseNm} Nm";
    }

    private static string BuildVerdict(CarDto car, CarListItemDto list) =>
        $"The {list.Brand} {list.Model} balances {list.FuelType.ToLowerInvariant()} efficiency, {list.Category.ToLowerInvariant()} practicality, and modern features for Indian buyers.";

    private static IReadOnlyList<string> BuildStrengths(CarDto car) =>
    [
        $"Strong {MapCategory(car).ToLowerInvariant()} appeal",
        $"{MapFuelType(car.FuelType)} efficiency",
        "Good feature-to-price ratio"
    ];

    private static IReadOnlyList<string> BuildWeaknesses(CarDto car) =>
    [
        "Premium variants can get expensive",
        "Real-world mileage varies by driving style",
        "Waiting periods may apply for popular trims"
    ];

    private static IReadOnlyList<string> BuildBestFor(CarDto car)
    {
        var category = MapCategory(car);
        return category switch
        {
            "SUV" => ["Families", "Highway travel", "City commuting"],
            "MPV" => ["Large families", "Long trips", "Fleet use"],
            "Coupe" => ["Enthusiasts", "City driving", "Weekend trips"],
            _ => ["Daily commute", "City driving", "First-time buyers"]
        };
    }

    private static string TitleCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));
    }

    private static double DeterministicRating(Guid id, double min, double max)
    {
        var value = DeterministicInt(id, "rating", 0, 1000) / 1000.0;
        return Math.Round(min + value * (max - min), 1);
    }

    private static int DeterministicInt(Guid id, string salt, int min, int max)
    {
        var hash = Hash($"{id}:{salt}");
        return min + Math.Abs(hash % (max - min + 1));
    }

    private static int Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToInt32(bytes, 0);
    }
}
