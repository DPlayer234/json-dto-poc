using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lib.Generated;

// Use a factory so we can bind the JsonTypeInfo<T> to the converter pre-emptively.

public sealed class JsonOptionalConverterFactory<T> : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(Optional<T>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!CanConvert(typeToConvert)) throw new ArgumentException("Cannot convert this type.", nameof(typeToConvert));
        return new JsonOptionalConverter<T>(options);
    }
}
