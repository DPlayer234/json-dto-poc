using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lib.Internal;

/// <summary>
/// This converter should never be invoked since it's only attached to an extension-data property.
/// </summary>
internal sealed class NoExtensionDataConverter : JsonConverter<NoExtensionData>
{
    public static readonly NoExtensionDataConverter Instance = new();

    public override NoExtensionData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, NoExtensionData value, JsonSerializerOptions options)
    {
        // There seems to be currently a bug where custom converters are invoked to write
        // extension data properties. We write nothing, as there are no properties for us to add.
    }
}
