using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lib.Internal;

internal sealed class ForwardingOptionalConverter<T> : JsonConverter<Optional<T>>
{
    private readonly JsonConverter<T> _inner;

    public ForwardingOptionalConverter(JsonConverter<T> inner)
    {
        _inner = inner;
    }

    public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(_inner.Read(ref reader, typeof(T), options)!);
    }

    public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
    {
        _inner.Write(writer, value.Value, options);
    }
}
