using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace SGen;

public sealed class SerializerConverterData
{
    public SerializerConverterData(INamedTypeSymbol contextType, ImmutableArray<ITypeSymbol> roots, ImmutableArray<ITypeSymbol> optionalTypes)
    {
        ContextType = contextType;
        Roots = roots;
        OptionalTypes = optionalTypes;
    }

    public INamedTypeSymbol ContextType { get; }
    public ImmutableArray<ITypeSymbol> Roots { get; }
    public ImmutableArray<ITypeSymbol> OptionalTypes { get; }
}
