using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Lib.Generated;
using Lib.Internal;

namespace Lib;

public sealed class JsonSerializerContextConfig
{
    private readonly List<IJsonTypeInfoResolver> _contexts = new();
    private readonly List<DataObjectConfig> _dtoConfigs = new();
    private Action<JsonTypeInfo>? _configureTypeInfos;

    public JsonSerializerOptions Options { get; private set; } = JsonSerializerOptions.Default;

    public ConfiguredJsonSerializerContext Build()
    {
        var options = new JsonSerializerOptions(Options);
        foreach (var dto in _dtoConfigs)
        {
            options.Converters.Add(dto.JsonInterfaceConverter);
        }

        foreach (var ctx in _contexts.OfType<IJsonSerializerContextAdditionalConverters>())
        {
            foreach (var converter in ctx.AdditionalConverters)
            {
                options.Converters.Add(converter);
            }
        }

        return new ConfiguredJsonSerializerContext(options, _contexts, _configureTypeInfos);
    }

    public JsonSerializerContextConfig WithOptions(JsonSerializerOptions options)
    {
        Options = options;
        return this;
    }

    public JsonSerializerContextConfig AddContext<TContext>() where TContext : IJsonTypeInfoResolver, new()
    {
        _contexts.Add(new TContext());
        return this;
    }

    public JsonSerializerContextConfig WithDataObject<TInterface, TModel>(Action<DataObjectConfig<TModel>>? configure = null)
        where TModel : TInterface
        where TInterface : class
    {
        DataObjectConfig<TModel> config = new(typeof(TInterface), new JsonInterfaceConverterFactory<TInterface, TModel>());
        configure?.Invoke(config);

        _configureTypeInfos += config.ApplyToTypeInfo;
        _dtoConfigs.Add(config);
        return this;
    }
}
