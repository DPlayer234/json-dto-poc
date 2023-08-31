namespace Lib;

public interface IOptional
{
    bool HasValue { get; }
    object? Value { get; }
}

public readonly struct Optional<T> : IOptional
{
    public Optional(T value)
    {
        Value = value;
        HasValue = true;
    }

    public bool HasValue { get; }
    public T Value { get; }

    object? IOptional.Value => Value;

    public override string ToString() => HasValue ? Value?.ToString() ?? string.Empty : "<Empty>";
}
