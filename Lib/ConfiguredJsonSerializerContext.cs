using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Lib.Internal;

namespace Lib;

public sealed class ConfiguredJsonSerializerContext : JsonSerializerContext, IJsonTypeInfoResolver
{
    private readonly ImmutableArray<IJsonTypeInfoResolver> _contexts;
    private readonly ImmutableArray<DataObjectConfig> _dtoConfigs;
    private readonly Action<JsonTypeInfo>? _configureTypeInfos;

    private readonly ConcurrentDictionary<Type, JsonTypeInfo?> _typeInfoCache = new();
    private readonly Func<Type, JsonSerializerOptions, JsonTypeInfo?> _getTypeInfoFromContexts;

    public ConfiguredJsonSerializerContext
    (
        JsonSerializerOptions options,
        IEnumerable<IJsonTypeInfoResolver> contexts,
        IEnumerable<DataObjectConfig> dtoConfigs,
        Action<JsonTypeInfo>? configureTypeInfos
    ) : base(options)
    {
        _contexts = contexts.ToImmutableArray();
        _dtoConfigs = dtoConfigs.ToImmutableArray();
        _configureTypeInfos = configureTypeInfos;

        _getTypeInfoFromContexts = GetTypeInfoFromContexts;
    }

    protected override JsonSerializerOptions? GeneratedSerializerOptions => null;

    public override JsonTypeInfo? GetTypeInfo(Type type)
    {
        return _typeInfoCache.GetOrAdd(type, _getTypeInfoFromContexts, Options);
    }

    public JsonTypeInfo<T>? GetTypeInfo<T>()
    {
        var typeInfo = GetTypeInfo(typeof(T));

        // Any correctly written ITypeInfoResolver will return the correct type here.
        Debug.Assert(typeInfo is null or JsonTypeInfo<T>);
        return Unsafe.As<JsonTypeInfo<T>>(typeInfo);
    }

    JsonTypeInfo? IJsonTypeInfoResolver.GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        return GetTypeInfoFromContexts(type, options);
    }

    private JsonTypeInfo? GetTypeInfoFromContexts(Type type, JsonSerializerOptions options)
    {
        foreach (var ctx in _contexts)
        {
            var info = ctx.GetTypeInfo(type, options);
            if (info != null)
            {
                _configureTypeInfos?.Invoke(info);
                return info;
            }
        }

        // Interfaces aren't automatically included, so generate them from the DTO config
        foreach (var dto in _dtoConfigs)
        {
            if (dto.InterfaceType == type)
            {
                return dto.CreateInterfaceTypeInfo(options);
            }
        }

        // This code essentially exists to allow the "no extra properties" to work in any case.
        // The string and object metadata is only provided for cases where those converters aren't already present.
        if (type == typeof(NoExtensionData))
        {
            return JsonMetadataServices.CreateValueInfo<NoExtensionData>(options, NoExtensionDataConverter.Instance);
        }

        if (type == typeof(string))
        {
            return JsonMetadataServices.CreateValueInfo<string>(options, JsonMetadataServices.StringConverter);
        }

        if (type == typeof(object))
        {
            return JsonMetadataServices.CreateValueInfo<object>(options, NoObjectConverter.Instance);
        }

        return null;
    }
}
