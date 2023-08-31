using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Lib.Internal;

internal sealed class NoExtensionData : IDictionary<string, object?>
{
    private readonly Type _type;

    public NoExtensionData(Type type)
    {
        _type = type;
    }

    public object? this[string key] { get => throw new KeyNotFoundException(); set => throw GetUnknownPropertyException(key); }

    public ICollection<string> Keys => Array.Empty<string>();
    public ICollection<object?> Values => Array.Empty<object?>();

    public int Count => 0;
    public bool IsReadOnly => false;

    public void Add(string key, object? value) => throw GetUnknownPropertyException(key);
    public void Add(KeyValuePair<string, object?> item) => throw GetUnknownPropertyException(item.Key);

    public bool Remove(string key) => false;
    public bool Remove(KeyValuePair<string, object?> item) => false;

    public void Clear() { }

    public bool Contains(KeyValuePair<string, object?> item) => false;
    public bool ContainsKey(string key) => false;

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) { }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object? value)
    {
        value = default;
        return false;
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => Enumerable.Empty<KeyValuePair<string, object?>>().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Enumerable.Empty<KeyValuePair<string, object?>>().GetEnumerator();

    private JsonException GetUnknownPropertyException(string property)
    {
        return new JsonException($"Unknown JSON property '{property}' for type '{_type}'.");
    }

    /// <summary> Disallows excess properties in the JSON payload. </summary>
    /// <remarks> This is only applicable to types without a custom converter and without extension data. </remarks>
    /// <param name="typeInfo"> The type info to modify. </param>
    public static void DisallowUnknownProperties(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object || typeInfo.Properties.Any(p => p.IsExtensionData))
            return;

        NoExtensionData sentinel = new(typeInfo.Type);

        var p = typeInfo.CreateJsonPropertyInfo(typeof(NoExtensionData), "$$extensiondata$$");
        p.IsExtensionData = true;
        p.Get = o => sentinel;
        p.Set = (o, v) => { /* Property needs to be writable to be deserialized even though it fills the return of Get. */ };

        typeInfo.Properties.Add(p);
    }
}
