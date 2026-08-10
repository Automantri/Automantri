namespace Automantri.Application.Cars;

/// <summary>
/// Normalizes free-text car search into tokens that can match across make/model/trim.
/// Supports queries like "alcazar", "hyundai alcazar", and common brand typos ("hyundi").
/// </summary>
public static class CarSearchText
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "and", "or", "for", "in", "of", "to", "car", "cars",
        "new", "used", "auto", "automobile", "vehicle", "vehicles", "india",
    };

    /// <summary>
    /// Common misspellings / aliases → canonical catalog token.
    /// </summary>
    private static readonly Dictionary<string, string> TokenAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["hyundi"] = "hyundai",
        ["hyunday"] = "hyundai",
        ["hundai"] = "hyundai",
        ["huyndai"] = "hyundai",
        ["suzuki"] = "maruti",
        ["maruthi"] = "maruti",
        ["vw"] = "volkswagen",
        ["volkswagon"] = "volkswagen",
        ["merc"] = "mercedes",
        ["mercedesbenz"] = "mercedes",
        ["mercedes-benz"] = "mercedes",
        ["benz"] = "mercedes",
        ["mahindra&mahindra"] = "mahindra",
        ["m&m"] = "mahindra",
        ["tata motors"] = "tata",
        ["skoda"] = "skoda",
        ["toyoto"] = "toyota",
        ["toyata"] = "toyota",
        ["hond"] = "honda",
    };

    public static IReadOnlyList<string> Tokenize(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return [];
        }

        var raw = search.Trim().ToLowerInvariant()
            .Replace('-', ' ')
            .Replace('_', ' ')
            .Replace('/', ' ');

        var tokens = raw
            .Split([' ', '\t', ',', ';', '+', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeToken)
            .Where(t => t.Length > 0 && !StopWords.Contains(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return tokens;
    }

    public static string NormalizePhrase(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return string.Empty;
        }

        return string.Join(' ', Tokenize(search));
    }

    private static string NormalizeToken(string token)
    {
        var cleaned = token.Trim().Trim('.', '\'', '"');
        if (cleaned.Length == 0)
        {
            return string.Empty;
        }

        // Collapse "mercedes-benz" style already split; handle glued aliases.
        var compact = cleaned.Replace(" ", string.Empty);
        if (TokenAliases.TryGetValue(cleaned, out var alias) ||
            TokenAliases.TryGetValue(compact, out alias))
        {
            return alias;
        }

        return cleaned;
    }
}
