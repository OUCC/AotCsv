﻿using Microsoft.CodeAnalysis;

namespace Oucc.AotCsv.Generator.Utility;

internal static class SymbolExtentions
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

        // TODO: ToString以外を使う
        return symbol?.ToString() == "<global namespace>";
    }
}