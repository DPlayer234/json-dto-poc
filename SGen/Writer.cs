﻿using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SGen;

internal static class Writer
{
    private const string OptionalConvType = "global::Lib.Generated.JsonOptionalConverterFactory";
    private const string AddConvInterface = "global::Lib.Generated.IJsonSerializerContextAdditionalConverters";

    internal static void Emit(SourceProductionContext spc, SerializerConverterData src)
    {
        StringBuilder builder = new();

        builder.Append($$"""
            // <autogenerated/>
            namespace {{src.ContextType.ContainingNamespace}};

            // Roots: {{string.Join(", ", src.Roots.Select(r => r.Name))}}

            """);

        builder.Append($$"""
            partial class {{src.ContextType.Name}} : {{AddConvInterface}}
            {
                System.Collections.Generic.IEnumerable<System.Text.Json.Serialization.JsonConverter> {{AddConvInterface}}.AdditionalConverters
                {
                    get
                    {

            """);

        if (src.OptionalTypes.IsEmpty)
        {
            builder.Append($$"""
                            yield break;

                """);
        }
        else
        {
            foreach (var type in src.OptionalTypes)
            {
                builder.Append($$"""
                            yield return new {{OptionalConvType}}<{{type}}>();

                """);
            }
        }


        builder.Append($$"""
                    }
                }
            }
            """);

        spc.AddSource($"{src.ContextType.Name}.AdditionalConverters.g.cs", builder.ToString());
    }
}
