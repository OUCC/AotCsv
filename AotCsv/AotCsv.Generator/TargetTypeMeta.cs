using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

internal class TargetTypeMeta
{
    public TargetTypeMeta(INamedTypeSymbol type)
    {
        Type = type;
        Name = type.ToDisplayString(SymbolFormat.NameOnly);
        HelperClassName = type.Arity == 0
            ? "Helper"
            : string.Concat("Helper<", string.Join(", ", type.TypeParameters.Select(t => t.ToDisplayString(SymbolFormat.NameOnly))), ">");
        HelperClassFullName = string.Concat(type.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), ".", HelperClassName);
    }

    public INamedTypeSymbol Type { get; }

    public string Name { get; }

    public string HelperClassFullName { get; }

    public string HelperClassName { get; }


}
