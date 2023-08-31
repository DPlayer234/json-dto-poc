using System.Text.Json.Serialization;

namespace Lib.Generated;

public interface IJsonSerializerContextAdditionalConverters
{
    IEnumerable<JsonConverter> AdditionalConverters { get; }
}
