using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automantri.Infrastructure.External.ApiNinjas;

internal sealed class FlexibleIntJsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Number when reader.TryGetInt32(out var value) => value,
            JsonTokenType.Number => Convert.ToInt32(reader.GetDouble()),
            JsonTokenType.String => ParseInt(reader.GetString()),
            JsonTokenType.Null => 0,
            _ => 0
        };
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }

    private static int ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var asDouble))
        {
            return Convert.ToInt32(asDouble);
        }

        return 0;
    }
}

internal sealed class FlexibleNullableIntJsonConverter : JsonConverter<int?>
{
    public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.Number when reader.TryGetInt32(out var value) => value,
            JsonTokenType.Number => Convert.ToInt32(reader.GetDouble()),
            JsonTokenType.String => ParseNullableInt(reader.GetString()),
            _ => null
        };
    }

    public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
            return;
        }

        writer.WriteNullValue();
    }

    private static int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var asDouble))
        {
            return Convert.ToInt32(asDouble);
        }

        return null;
    }
}

internal sealed class FlexibleNullableDecimalJsonConverter : JsonConverter<decimal?>
{
    public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.Number when reader.TryGetDecimal(out var value) => value,
            JsonTokenType.Number => Convert.ToDecimal(reader.GetDouble()),
            JsonTokenType.String => ParseNullableDecimal(reader.GetString()),
            _ => null
        };
    }

    public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteNumberValue(value.Value);
            return;
        }

        writer.WriteNullValue();
    }

    private static decimal? ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}
