using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lib.Internal;

internal sealed class NoObjectConverter : JsonConverter<object>
{
    public static readonly NoObjectConverter Instance = new();

    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // It's reasonable to assume this converter is only invoked if none of the contexts
        // contain `object` in their graph. As such, we just return null and hope this ends
        // up at `NoExtensionData`. If not, well, cryptic error.

        return null;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
