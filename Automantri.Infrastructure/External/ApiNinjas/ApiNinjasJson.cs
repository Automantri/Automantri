using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automantri.Infrastructure.External.ApiNinjas;

internal static class ApiNinjasJson
{
    public static JsonSerializerOptions SerializerOptions { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new FlexibleIntJsonConverter());
        options.Converters.Add(new FlexibleNullableIntJsonConverter());
        options.Converters.Add(new FlexibleNullableDecimalJsonConverter());
        return options;
    }
}
