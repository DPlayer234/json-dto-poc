using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Lib.Internal;

namespace Lib;

public abstract class DataObjectConfig
{
    public abstract Type InterfaceType { get; }
    public abstract Type ModelType { get; }

    public abstract JsonConverter JsonInterfaceConverter { get; }
}

public sealed class DataObjectConfig<TModel> : DataObjectConfig
{
    private readonly Dictionary<string, PropertyConfig> _properties = new();

    internal DataObjectConfig(Type interfaceType, JsonConverter jsonInterfaceConverter)
    {
        InterfaceType = interfaceType;
        JsonInterfaceConverter = jsonInterfaceConverter;
    }

    public override Type InterfaceType { get; }
    public override Type ModelType => typeof(TModel);

    public override JsonConverter JsonInterfaceConverter { get; }

    public void SetPropertyName(Expression<Func<TModel, object>> property, string name)
    {
        string propertyName = GetPropertyName(property);
        CollectionsMarshal.GetValueRefOrAddDefault(_properties, propertyName, out _).Name = name;
    }

    public void SetPropertyConverter(Expression<Func<TModel, object>> property, JsonConverter converter)
    {
        string propertyName = GetPropertyName(property);
        CollectionsMarshal.GetValueRefOrAddDefault(_properties, propertyName, out _).Converter = converter;
    }

    internal void ApplyToTypeInfo(JsonTypeInfo jsonTypeInfo)
    {
        if (typeof(TModel) != jsonTypeInfo.Type) return;

        foreach (var (rawName, config) in _properties)
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

    private struct PropertyConfig
    {
        public string? Name;
        public JsonConverter? Converter;
    }

    private static string GetPropertyName(Expression<Func<TModel, object>> property)
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
