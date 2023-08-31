using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lib.Internal;

// Use a factory so we can bind the JsonTypeInfo<T> to the converter pre-emptively.

internal sealed class JsonInterfaceConverterFactory<TInterface, TModel> : JsonConverterFactory
    where TModel : TInterface
    where TInterface : class
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(TInterface) == typeToConvert;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!CanConvert(typeToConvert)) throw new ArgumentException("Cannot convert this type.", nameof(typeToConvert));
        return new JsonInterfaceConverter<TInterface, TModel>(options);
    }
}
