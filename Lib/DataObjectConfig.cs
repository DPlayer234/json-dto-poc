using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Lib.Internal;

namespace Lib;

public abstract class DataObjectConfig
{
    public abstract Type InterfaceType { get; }
    public abstract Type ModelType { get; }
    public abstract JsonConverter JsonInterfaceConverter { get; }

    public abstract JsonTypeInfo CreateInterfaceTypeInfo(JsonSerializerOptions options);
}

public abstract class DataObjectConfig<TModel> : DataObjectConfig
{
    internal readonly Dictionary<string, PropertyConfig> properties = new();

    public void SetPropertyName(Expression<Func<TModel, object?>> property, string name)
    {
        string propertyName = GetPropertyName(property);
        CollectionsMarshal.GetValueRefOrAddDefault(properties, propertyName, out _).Name = name;
    }

    public void SetPropertyConverter(Expression<Func<TModel, object?>> property, JsonConverter converter)
    {
        string propertyName = GetPropertyName(property);
        CollectionsMarshal.GetValueRefOrAddDefault(properties, propertyName, out _).Converter = converter;
    }

    public void SetPropertyConverter<TProperty>(Expression<Func<TModel, TProperty?>> property, JsonConverter<TProperty> converter)
        where TProperty : struct
    {
        string propertyName = GetPropertyName(property);
        CollectionsMarshal.GetValueRefOrAddDefault(properties, propertyName, out _).Converter = new ForwardingNullableConverter<TProperty>(converter);
    }

    public void SetPropertyConverter<TProperty>(Expression<Func<TModel, Optional<TProperty>>> property, JsonConverter<TProperty> converter)
        where TProperty : struct
    {
        string propertyName = GetPropertyName(property);
        CollectionsMarshal.GetValueRefOrAddDefault(properties, propertyName, out _).Converter = new ForwardingOptionalConverter<TProperty>(converter);
    }

    internal struct PropertyConfig
    {
        public string? Name;
        public JsonConverter? Converter;
    }

    private static string GetPropertyName<T>(Expression<Func<TModel, T>> property)
    {
        var body = property.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert, Operand: var inner })
        {
            body = inner;
        }

        if (body is MemberExpression { Member: PropertyInfo member })
        {
            return member.Name;
        }

        throw new ArgumentException("Expression does not represent a property.", nameof(property));
    }
}

internal sealed class DataObjectConfig<TInterface, TModel> : DataObjectConfig<TModel>
    where TModel : TInterface
    where TInterface : class
{
    public override Type InterfaceType => typeof(TInterface);
    public override Type ModelType => typeof(TModel);

    public override JsonConverter JsonInterfaceConverter { get; } = new JsonInterfaceConverterFactory<TInterface, TModel>();

    public override JsonTypeInfo CreateInterfaceTypeInfo(JsonSerializerOptions options)
    {
        return JsonMetadataServices.CreateValueInfo<TInterface>(options, new JsonInterfaceConverter<TInterface, TModel>(options));
    }

    internal void ApplyToTypeInfo(JsonTypeInfo jsonTypeInfo)
    {
        if (typeof(TModel) != jsonTypeInfo.Type) return;

        foreach (var (rawName, config) in properties)
        {
            var convertedName = jsonTypeInfo.Options.PropertyNamingPolicy?.ConvertName(rawName) ?? rawName;
            var property = jsonTypeInfo.Properties.First(p => p.Name == convertedName);

            if (config.Name != null)
            {
                property.Name = config.Name;
            }

            if (config.Converter != null)
            {
                property.CustomConverter = config.Converter;
            }
        }

        foreach (var property in jsonTypeInfo.Properties)
        {
            if (typeof(IOptional).IsAssignableFrom(property.PropertyType))
            {
                property.IsRequired = false;
                property.ShouldSerialize = (o, v) => v is IOptional { HasValue: true };
            }
            else
            {
                property.IsRequired = true;
            }
        }

        NoExtensionData.DisallowUnknownProperties(jsonTypeInfo);
    }
}
