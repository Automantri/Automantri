using System.Text.Json;
using Automantri.Application.Cars;
using Automantri.Application.Common.Interfaces;
using Automantri.Application.Imports;
using Automantri.Domain.Entities;
using ClosedXML.Excel;

namespace Automantri.Infrastructure.Imports;

public sealed class CatalogImportService(ICarRepository carRepository) : ICatalogImportService
{
    private static readonly HashSet<string> TransmissionTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "MT", "AT", "IVT", "DCT", "AMT", "CVT", "AWD", "MT/IVT", "MT/DCT", "MT/AT", "MT / DCT", "DCT Only", "IVT Only"
    };

    public async Task<CatalogImportPreviewResultDto> PreviewAsync(
        Stream workbookStream,
        string fileName,
        CatalogImportPreviewRequest request,
        CancellationToken cancellationToken)
    {
        var brand = string.IsNullOrWhiteSpace(request.Brand) ? "Hyundai" : request.Brand.Trim();
        var year = request.Year <= 0 ? DateTime.UtcNow.Year : request.Year;
        var parsed = ParseWorkbook(workbookStream, brand, year);
        var sheets = parsed.Select(r => r.SheetName ?? r.Model).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        var existing = await carRepository.GetAllAsync(cancellationToken);
        var brandCars = existing
            .Where(c => c.Make.Equals(brand, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var rows = new List<CatalogImportRowDto>();
        var matchedExistingIds = new HashSet<Guid>();

        foreach (var item in parsed)
        {
            var match = brandCars.FirstOrDefault(c =>
                c.Model.Equals(item.Model, StringComparison.OrdinalIgnoreCase) &&
                c.Year == item.Year &&
                string.Equals(Normalize(c.Trim), Normalize(item.Variant), StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Normalize(c.Transmission), Normalize(item.Transmission), StringComparison.OrdinalIgnoreCase));

            if (match is not null)
            {
                matchedExistingIds.Add(match.Id);
                rows.Add(item with
                {
                    Action = "update",
                    ExistingId = match.Id,
                });
            }
            else
            {
                rows.Add(item with { Action = "create", ExistingId = null });
            }
        }

        if (request.SyncDeletes)
        {
            var importModels = parsed
                .Select(r => r.Model)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var car in brandCars.Where(c => importModels.Contains(c.Model) && c.Year == year && !matchedExistingIds.Contains(c.Id)))
            {
                rows.Add(new CatalogImportRowDto(
                    ClientId: Guid.NewGuid().ToString("N"),
                    Brand: brand,
                    Model: car.Model,
                    Year: car.Year,
                    Variant: car.Trim ?? "Base",
                    Transmission: car.Transmission,
                    FuelType: car.FuelType,
                    VehicleClass: car.VehicleClass,
                    Engines: null,
                    Features: [],
                    Action: "delete",
                    ExistingId: car.Id,
                    SheetName: car.Model));
            }
        }

        return new CatalogImportPreviewResultDto(
            brand,
            year,
            sheets,
            rows,
            rows.Count(r => r.Action == "create"),
            rows.Count(r => r.Action == "update"),
            rows.Count(r => r.Action == "delete"));
    }

    public async Task<CatalogImportCommitResultDto> CommitAsync(
        CatalogImportCommitRequest request,
        CancellationToken cancellationToken)
    {
        var brand = string.IsNullOrWhiteSpace(request.Brand) ? "Hyundai" : request.Brand.Trim();
        var year = request.Year <= 0 ? DateTime.UtcNow.Year : request.Year;
        var now = DateTimeOffset.UtcNow;

        var upserts = new List<Car>();
        var deleteIds = new List<Guid>();
        var skipped = 0;

        foreach (var row in request.Rows)
        {
            var action = (row.Action ?? "create").Trim().ToLowerInvariant();
            switch (action)
            {
                case "create":
                case "update":
                    upserts.Add(ToEntity(row, brand, year, now));
                    break;
                case "delete":
                    if (row.ExistingId is Guid id)
                    {
                        deleteIds.Add(id);
                    }
                    else
                    {
                        skipped++;
                    }
                    break;
                case "skip":
                case "unchanged":
                    skipped++;
                    break;
                default:
                    upserts.Add(ToEntity(row, brand, year, now));
                    break;
            }
        }

        var result = await carRepository.UpsertRangeAsync(upserts, cancellationToken);
        var deleted = await carRepository.DeleteByIdsAsync(deleteIds, cancellationToken);
        await carRepository.SaveChangesAsync(cancellationToken);

        return new CatalogImportCommitResultDto(
            result.InsertedCount,
            result.UpdatedCount,
            deleted,
            skipped,
            $"Import complete: {result.InsertedCount} added, {result.UpdatedCount} updated, {deleted} deleted.");
    }

    private static Car ToEntity(CatalogImportRowDto row, string brand, int fallbackYear, DateTimeOffset now)
    {
        var year = row.Year > 0 ? row.Year : fallbackYear;
        var trim = Clip(string.IsNullOrWhiteSpace(row.Variant) ? "Base" : row.Variant.Trim(), 200);
        var make = Clip(string.IsNullOrWhiteSpace(row.Brand) ? brand : row.Brand.Trim(), 120);
        var model = Clip(row.Model.Trim(), 120);
        var specs = JsonSerializer.Serialize(new
        {
            importSource = "xlsx-admin",
            engines = row.Engines,
            features = row.Features,
        });

        var car = new Car
        {
            Make = make,
            Model = model,
            Year = year,
            Trim = trim,
            Transmission = NormalizeTransmission(row.Transmission),
            FuelType = Clip(string.IsNullOrWhiteSpace(row.FuelType) ? GuessFuel(row) : row.FuelType.Trim(), 50),
            VehicleClass = Clip(string.IsNullOrWhiteSpace(row.VehicleClass) ? GuessClass(model) : row.VehicleClass.Trim(), 120),
            CarType = Clip(GuessClass(model), 50),
            CityMpg = 0,
            HighwayMpg = 0,
            CombinationMpg = 0,
            SpecificationsJson = specs,
            SourceQuery = Clip($"xlsx-import:{make}/{model}/{year}/{trim}", 250),
            RetrievedAtUtc = now,
            UpdatedAtUtc = now,
        };

        CarIdentity.NormalizeCar(car);
        return car;
    }

    private static IReadOnlyList<CatalogImportRowDto> ParseWorkbook(Stream stream, string brand, int year)
    {
        using var workbook = new XLWorkbook(stream);
        var rows = new List<CatalogImportRowDto>();

        foreach (var worksheet in workbook.Worksheets)
        {
            rows.AddRange(ParseSheet(worksheet, brand, year));
        }

        return rows
            .GroupBy(r => $"{r.Brand}|{r.Model}|{r.Year}|{r.Variant}|{r.Transmission}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToArray();
    }

    private static IEnumerable<CatalogImportRowDto> ParseSheet(IXLWorksheet sheet, string brand, int year)
    {
        var used = sheet.RangeUsed();
        if (used is null)
        {
            yield break;
        }

        var matrix = used.RowsUsed()
            .Select(r => r.Cells(1, used.ColumnCount())
                .Select(c => CellText(c.GetString()))
                .ToArray())
            .Where(r => r.Any(v => !string.IsNullOrWhiteSpace(v)))
            .ToList();

        if (matrix.Count < 2)
        {
            yield break;
        }

        var model = ExtractModelName(matrix, sheet.Name);
        var (variantRowIndex, variants) = FindVariants(matrix);
        if (variants.Count == 0)
        {
            // Feature-only sheets like Ioniq 5 — single base variant
            variants = [new VariantColumn(2, "Base")];
            variantRowIndex = 0;
        }

        var engineNotes = new List<string>();
        var featuresByVariant = variants.ToDictionary(v => v.ColumnIndex, _ => new List<string>());
        var transmissionByVariant = variants.ToDictionary(v => v.ColumnIndex, _ => (string?)null);
        var fuelHints = new List<string>();

        for (var i = 0; i < matrix.Count; i++)
        {
            if (i == variantRowIndex)
            {
                continue;
            }

            var row = matrix[i];
            var col0 = GetCell(row, 0);
            var col1 = GetCell(row, 1);

            if (LooksLikeModelHeader(col0) || IsSectionHeader(col0) || IsSectionHeader(col1))
            {
                if (IsFeaturesHeader(col0) || IsFeaturesHeader(col1))
                {
                    // continue collecting features after this
                }
                continue;
            }

            if (LooksLikeEngineRow(col0, col1) || LooksLikeEngineRow(col1, col0))
            {
                var engineLabel = string.Join(" ", new[] { col0, col1 }.Where(s => !string.IsNullOrWhiteSpace(s)));
                if (!string.IsNullOrWhiteSpace(engineLabel))
                {
                    engineNotes.Add(engineLabel);
                    if (engineLabel.Contains("diesel", StringComparison.OrdinalIgnoreCase)) fuelHints.Add("Diesel");
                    if (engineLabel.Contains("petrol", StringComparison.OrdinalIgnoreCase) ||
                        engineLabel.Contains("gdi", StringComparison.OrdinalIgnoreCase) ||
                        engineLabel.Contains("mpi", StringComparison.OrdinalIgnoreCase)) fuelHints.Add("Petrol");
                    if (engineLabel.Contains("cng", StringComparison.OrdinalIgnoreCase)) fuelHints.Add("CNG");
                    if (engineLabel.Contains("ev", StringComparison.OrdinalIgnoreCase) ||
                        engineLabel.Contains("kwh", StringComparison.OrdinalIgnoreCase) ||
                        engineLabel.Contains("electric", StringComparison.OrdinalIgnoreCase)) fuelHints.Add("Electric");
                }

                foreach (var variant in variants)
                {
                    var cell = GetCell(row, variant.ColumnIndex);
                    if (IsUnavailable(cell))
                    {
                        continue;
                    }

                    if (IsTransmissionToken(cell))
                    {
                        transmissionByVariant[variant.ColumnIndex] = NormalizeTransmission(cell);
                    }
                }

                continue;
            }

            // Feature row
            var featureName = BuildFeatureName(col0, col1);
            if (string.IsNullOrWhiteSpace(featureName) ||
                featureName.Equals("features", StringComparison.OrdinalIgnoreCase) ||
                featureName.Equals("feature", StringComparison.OrdinalIgnoreCase) ||
                featureName.Equals("variants", StringComparison.OrdinalIgnoreCase) ||
                featureName.Equals("variant", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var variant in variants)
            {
                var cell = GetCell(row, variant.ColumnIndex);
                if (IsAvailable(cell))
                {
                    var label = IsTransmissionToken(cell)
                        ? $"{featureName} ({cell})"
                        : featureName;
                    featuresByVariant[variant.ColumnIndex].Add(label);
                    if (IsTransmissionToken(cell) && string.IsNullOrWhiteSpace(transmissionByVariant[variant.ColumnIndex]))
                    {
                        transmissionByVariant[variant.ColumnIndex] = NormalizeTransmission(cell);
                    }
                }
            }
        }

        var fuel = fuelHints.Distinct(StringComparer.OrdinalIgnoreCase).FirstOrDefault()
                   ?? (model.Contains("EV", StringComparison.OrdinalIgnoreCase) || model.Contains("Ioniq", StringComparison.OrdinalIgnoreCase)
                       ? "Electric"
                       : "Petrol");
        var engines = string.Join("; ", engineNotes.Distinct(StringComparer.OrdinalIgnoreCase).Take(8));

        foreach (var variant in variants)
        {
            var transmission = transmissionByVariant[variant.ColumnIndex];
            // If no positive signal in engine/feature cells for this variant and matrix is sparse, still import
            yield return new CatalogImportRowDto(
                ClientId: Guid.NewGuid().ToString("N"),
                Brand: brand,
                Model: model,
                Year: year,
                Variant: variant.Name,
                Transmission: transmission,
                FuelType: fuel,
                VehicleClass: GuessClass(model),
                Engines: string.IsNullOrWhiteSpace(engines) ? null : engines,
                Features: featuresByVariant[variant.ColumnIndex].Distinct(StringComparer.OrdinalIgnoreCase).Take(80).ToArray(),
                Action: "create",
                ExistingId: null,
                SheetName: sheet.Name);
        }
    }

    private static string ExtractModelName(List<string[]> matrix, string sheetName)
    {
        foreach (var row in matrix.Take(3))
        {
            for (var i = 0; i < row.Length; i++)
            {
                if (LooksLikeModelHeader(row[i]) && i + 1 < row.Length && !string.IsNullOrWhiteSpace(row[i + 1]))
                {
                    return CleanModel(row[i + 1]);
                }
            }

            if (row.Length > 1 && !string.IsNullOrWhiteSpace(row[1]) && LooksLikeModelHeader(row[0]))
            {
                return CleanModel(row[1]);
            }
        }

        return CleanModel(sheetName);
    }

    private static (int RowIndex, List<VariantColumn> Variants) FindVariants(List<string[]> matrix)
    {
        for (var i = 0; i < Math.Min(matrix.Count, 8); i++)
        {
            var row = matrix[i];
            var header = GetCell(row, 0);
            if (!IsVariantHeader(header) && !IsVariantHeader(GetCell(row, 1)))
            {
                // Also allow row with multiple variant-like labels even without explicit "variant"
                var maybe = ExtractVariantColumns(row);
                if (maybe.Count >= 2 && i <= 3)
                {
                    return (i, maybe);
                }
                continue;
            }

            var variants = ExtractVariantColumns(row);
            if (variants.Count > 0)
            {
                return (i, variants);
            }
        }

        // Creta EV style: row index 2 often has variants starting at col 3
        if (matrix.Count > 2)
        {
            var forced = ExtractVariantColumns(matrix[2], startColumn: 2);
            if (forced.Count > 0)
            {
                return (2, forced);
            }
        }

        return (0, []);
    }

    private static List<VariantColumn> ExtractVariantColumns(string[] row, int startColumn = 2)
    {
        var variants = new List<VariantColumn>();
        for (var col = startColumn; col < row.Length; col++)
        {
            var name = GetCell(row, col);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (name.Equals("features", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("feature", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            variants.Add(new VariantColumn(col, CleanModel(name)));
        }

        return variants;
    }

    private static string BuildFeatureName(string col0, string col1)
    {
        if (!string.IsNullOrWhiteSpace(col0) && !string.IsNullOrWhiteSpace(col1))
        {
            return $"{col0.Trim()} - {col1.Trim()}";
        }

        return (string.IsNullOrWhiteSpace(col0) ? col1 : col0).Trim();
    }

    private static bool LooksLikeEngineRow(string a, string b)
    {
        var text = $"{a} {b}".ToLowerInvariant();
        return text.Contains("petrol") ||
               text.Contains("diesel") ||
               text.Contains("turbo") ||
               text.Contains("kappa") ||
               text.Contains("gdi") ||
               text.Contains("crdi") ||
               text.Contains("kwh") ||
               text.Contains("battery") ||
               text.Contains("u2 ") ||
               text.Contains("mpi") ||
               text.Contains("bi-fuel") ||
               text.Contains("hybrid");
    }

    private static bool LooksLikeModelHeader(string value) =>
        value.Contains("car model", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("model", StringComparison.OrdinalIgnoreCase);

    private static bool IsVariantHeader(string value) =>
        value.Contains("variant", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("varaint", StringComparison.OrdinalIgnoreCase); // typo in Aura sheet

    private static bool IsFeaturesHeader(string value) =>
        value.Equals("features", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("feature", StringComparison.OrdinalIgnoreCase);

    private static bool IsSectionHeader(string value) =>
        IsFeaturesHeader(value) ||
        value.Equals("general", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("battery pack", StringComparison.OrdinalIgnoreCase);

    private static bool IsUnavailable(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        var v = value.Trim();
        return v.Equals("NO", StringComparison.OrdinalIgnoreCase) ||
               v.Equals("N", StringComparison.OrdinalIgnoreCase) ||
               v == "-" ||
               v == "—";
    }

    private static bool IsAvailable(string value)
    {
        if (IsUnavailable(value)) return false;
        if (value.Equals("YES", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Y", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Yes*", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return IsTransmissionToken(value) || value.Length > 0;
    }

    private static bool IsTransmissionToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var cleaned = value.Trim().Replace(" ", "").Replace("Only", "", StringComparison.OrdinalIgnoreCase);
        if (TransmissionTokens.Contains(value.Trim()) || TransmissionTokens.Contains(cleaned))
        {
            return true;
        }

        // Exact short codes only — avoid matching feature text like "Seat" / "Battery" via "AT".
        return System.Text.RegularExpressions.Regex.IsMatch(
            cleaned,
            @"^(MT|AT|IVT|DCT|AMT|CVT|AWD)(/(MT|AT|IVT|DCT|AMT|CVT|AWD))*$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string? NormalizeTransmission(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleaned = value.Trim().Replace(" ", "");
        cleaned = cleaned.Replace("Only", "", StringComparison.OrdinalIgnoreCase);
        return Clip(cleaned, 80);
    }

    private static string Clip(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }

    private static string GuessClass(string model)
    {
        var m = model.ToLowerInvariant();
        if (m.Contains("ioniq") || m.Contains("ev")) return "Electric";
        if (m.Contains("creta") || m.Contains("venue") || m.Contains("tucson") || m.Contains("alcazar") || m.Contains("exter"))
            return "SUV";
        if (m.Contains("i10") || m.Contains("i20") || m.Contains("exter")) return "Hatchback";
        if (m.Contains("verna") || m.Contains("aura")) return "Sedan";
        return "Car";
    }

    private static string GuessFuel(CatalogImportRowDto row)
    {
        if (!string.IsNullOrWhiteSpace(row.FuelType)) return row.FuelType;
        var engines = row.Engines ?? string.Empty;
        if (engines.Contains("diesel", StringComparison.OrdinalIgnoreCase)) return "Diesel";
        if (engines.Contains("cng", StringComparison.OrdinalIgnoreCase)) return "CNG";
        if (row.Model.Contains("EV", StringComparison.OrdinalIgnoreCase) ||
            row.Model.Contains("Ioniq", StringComparison.OrdinalIgnoreCase)) return "Electric";
        return "Petrol";
    }

    private static string CleanModel(string value) =>
        value.Replace('\n', ' ').Replace("_", " ").Trim();

    private static string GetCell(string[] row, int index) =>
        index >= 0 && index < row.Length ? row[index].Trim() : string.Empty;

    private static string CellText(string value) =>
        (value ?? string.Empty).Replace('\n', ' ').Trim();

    private static string Normalize(string? value) =>
        (value ?? string.Empty).Trim().ToLowerInvariant();

    private sealed record VariantColumn(int ColumnIndex, string Name);
}
