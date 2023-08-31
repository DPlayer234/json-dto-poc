using System.Text.Json.Serialization;

namespace App;

[JsonSerializable(typeof(SomeData))]
public partial class SomeJsonContext : JsonSerializerContext
{
}

[JsonSerializable(typeof(SomeOtherData))]
public partial class SomeOtherJsonContext : JsonSerializerContext
{
}
