using Microsoft.CodeAnalysis;

namespace Oucc.AotCsv.Generator.Utility;

internal static class SymbolExtensions
{
    public static bool EqualsByMetadataName(this ITypeSymbol? typeSymbol, ReadOnlySpan<string> fullyQualifiedMetadataName)
    {
        INamespaceOrTypeSymbol? symbol = typeSymbol;

        for (var i = fullyQualifiedMetadataName.Length - 1; i >= 0; i--)
        {
            if (symbol is null || symbol.MetadataName != fullyQualifiedMetadataName[i])
                return false;

            symbol = symbol.ContainingType as INamespaceOrTypeSymbol ?? symbol.ContainingNamespace;
        }

        return symbol is INamespaceSymbol namespaceSymbol && namespaceSymbol.IsGlobalNamespace;
    }
}
