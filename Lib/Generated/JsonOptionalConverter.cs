using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Lib.Generated;

internal sealed class JsonOptionalConverter<T> : JsonConverter<Optional<T>>
{
    private JsonTypeInfo<T>? _jsonTypeInfo;
    private readonly JsonSerializerOptions _options;

    public JsonOptionalConverter(JsonSerializerOptions options)
    {
        _options = options;
    }

    internal JsonTypeInfo<T> JsonTypeInfo => _jsonTypeInfo ??= (JsonTypeInfo<T>)_options.GetTypeInfo(typeof(T));

    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(JsonSerializer.Deserialize(ref reader, JsonTypeInfo)!);
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        // This already handles nulls
        JsonSerializer.Serialize(writer, value.Value, JsonTypeInfo);
    }
}
