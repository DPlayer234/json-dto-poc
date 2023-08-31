using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Lib.Internal;

internal sealed class JsonInterfaceConverter<TInterface, TModel> : JsonConverter<TInterface>
    where TModel : TInterface
    where TInterface : class
{
    private JsonTypeInfo<TModel>? _jsonTypeInfo;
    private readonly JsonSerializerOptions _options;

    public JsonInterfaceConverter(JsonSerializerOptions options)
    {
        _options = options;
    }

    internal JsonTypeInfo<TModel> JsonTypeInfo => _jsonTypeInfo ??= (JsonTypeInfo<TModel>)_options.GetTypeInfo(typeof(TModel));

    public override TInterface? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize(ref reader, JsonTypeInfo);
    }

    public override void Write(Utf8JsonWriter writer, TInterface value, JsonSerializerOptions options)
    {
        var realType = value.GetType();
        var typeInfo = realType == typeof(TModel) ? JsonTypeInfo : options.GetTypeInfo(realType);

        // use the type information of the concrete type, or fall back to the interface
        // the use of Unsafe.As here is a little scary, but it's just to trick the compiler into accepting the
        // type info. Under the hood, in IL, the two values are compatible since we know for a fact that the real type
        // of value is TReal, and the runtime type of typeInfo is JsonTypeInfo<TReal>.
        //
        JsonSerializer.Serialize(writer, value, Unsafe.As<JsonTypeInfo<TInterface>>(typeInfo));
    }
}
