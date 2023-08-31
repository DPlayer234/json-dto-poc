using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SGen;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider
            (
                predicate: static (s, _) => s is ClassDeclarationSyntax,
                transform: static (ctx, _) => TransformSerializerContext(ctx)!
            )
            .Where(x => x is not null);

        context.RegisterSourceOutput
        (
            source: provider,
            action: static (spc, src) => Writer.Emit(spc, src)
        );
    }

    private static SerializerConverterData? TransformSerializerContext(GeneratorSyntaxContext ctx)
    {
        // TODO: Properly check the exact type instead of just their name.

        var typeDecl = (ClassDeclarationSyntax)ctx.Node;
        var type = (INamedTypeSymbol)ctx.SemanticModel.GetDeclaredSymbol(typeDecl)!;
        if (!IsPrimaryDeclaration(type, typeDecl)) return null;
        if (type.BaseType?.Name != "JsonSerializerContext") return null;

        var roots = type.GetAttributes()
            .Where(a => a.AttributeClass?.Name == "JsonSerializableAttribute")
            .Select(a => (ITypeSymbol)a.ConstructorArguments[0].Value!)
            .Where(x => x is not null)
            .ToImmutableArray();

        HashSet<ITypeSymbol> referenced = new(SymbolEqualityComparer.Default);
        foreach (var root in roots)
        {
            ExpandReferencedTypes(root, referenced);
        }

        var optionalTypes = referenced
            .OfType<INamedTypeSymbol>()
            .Where(t => t.IsValueType && t.IsGenericType && t.Name == "Optional")
            .Select(t => t.TypeArguments[0])
            .ToImmutableArray();

        return new SerializerConverterData(type, roots, optionalTypes);
    }

    private static void ExpandReferencedTypes(ITypeSymbol symbol, HashSet<ITypeSymbol> foundTypes)
    {
        // If it has already been seen, no need to check it again.
        if (!foundTypes.Add(symbol)) return;

        if (symbol.BaseType is { } @base)
        {
            ExpandReferencedTypes(@base, foundTypes);
        }

        foreach (var @interface in symbol.Interfaces)
        {
            ExpandReferencedTypes(@interface, foundTypes);
        }

        foreach (var member in symbol.GetMembers())
        {
            // We only care about public properties for now
            if (member.DeclaredAccessibility == Accessibility.Public &&
                member is IPropertySymbol property)
            {
                ExpandReferencedTypes(property.Type, foundTypes);
            }
        }
    }

    private static bool IsPrimaryDeclaration(ISymbol symbol, SyntaxNode syntax)
    {
        var decl = symbol.DeclaringSyntaxReferences;
        if (decl.Length <= 1) return true;

        var first = decl[0];
        return first.SyntaxTree == syntax.SyntaxTree
            && first.Span == syntax.Span;
    }
}
