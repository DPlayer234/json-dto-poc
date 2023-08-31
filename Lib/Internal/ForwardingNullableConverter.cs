using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lib.Internal;

internal sealed class ForwardingNullableConverter<T> : JsonConverter<T?>
    where T : struct
{
    private readonly JsonConverter<T> _inner;

    public ForwardingNullableConverter(JsonConverter<T> inner)
    {
        _inner = inner;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Null)
        {
            return _inner.Read(ref reader, typeof(T), options);
        }
        else
        {
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            _inner.Write(writer, value.GetValueOrDefault(), options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
