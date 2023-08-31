using Lib;

namespace App;

public interface ISomeData
{
    int Number { get; }
    string Text { get; }
    Optional<ISomeData> Next { get; }
}

public interface ISomeOtherData
{
    int Value { get; }
}

public record SomeData
(
    int Number,
    string Text,
    Optional<ISomeData> Next = default
) : ISomeData;

public record SomeOtherData
(
    int Value
) : ISomeOtherData;
