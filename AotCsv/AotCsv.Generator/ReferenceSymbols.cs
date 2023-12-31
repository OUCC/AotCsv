﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

// 参考：MemoryPack https://github.com/Cysharp/MemoryPack/blob/main/src/MemoryPack.Generator/ReferenceSymbols.cs

namespace Oucc.AotCsv.Generator;

internal class ReferenceSymbols
{
    public Compilation Compilation { get; }

    internal INamedTypeSymbol ISpanFormattable { get; }
    internal INamedTypeSymbol IFormattable { get; }
    internal INamedTypeSymbol Nullable_T { get; }
    internal INamedTypeSymbol DateTime { get; }
    internal INamedTypeSymbol String { get; }
    internal INamedTypeSymbol Boolean { get; }
    internal INamedTypeSymbol Char { get; }
    internal INamedTypeSymbol CsvDateTimeFormatAttribute { get; }
    internal INamedTypeSymbol CsvIncludeAttribute { get; }
    internal INamedTypeSymbol CsvIgnoreAttribute { get; }
    internal INamedTypeSymbol CsvIndexAttribute { get; }
    internal INamedTypeSymbol CsvNameAttribute { get; }

    internal ReferenceSymbols(Compilation compilation)
    {
        Compilation = compilation;
        ISpanFormattable = GetTypeByMetadataName(compilation, "System.ISpanFormattable");
        IFormattable = GetTypeByMetadataName(compilation, "System.IFormattable");
        Nullable_T = GetTypeByMetadataName(compilation, "System.Nullable`1").ConstructUnboundGenericType();
        DateTime = GetTypeByMetadataName(compilation, "System.DateTime");
        String = GetTypeByMetadataName(compilation, "System.String");
        Boolean = GetTypeByMetadataName(compilation, "System.Boolean");
        Char = GetTypeByMetadataName(compilation, "System.Char");
        CsvDateTimeFormatAttribute = GetTypeByMetadataName(compilation, "Oucc.AotCsv.Attributes.CsvDateTimeFormatAttribute");
        CsvIncludeAttribute = GetTypeByMetadataName(compilation, "Oucc.AotCsv.Attributes.CsvIncludeAttribute");
        CsvIgnoreAttribute = GetTypeByMetadataName(compilation, "Oucc.AotCsv.Attributes.CsvIgnoreAttribute");
        CsvIndexAttribute = GetTypeByMetadataName(compilation, "Oucc.AotCsv.Attributes.CsvIndexAttribute");
        CsvNameAttribute = GetTypeByMetadataName(compilation, "Oucc.AotCsv.Attributes.CsvNameAttribute");
    }

    private static INamedTypeSymbol GetTypeByMetadataName(Compilation compilation, string metadataName)
    {
        var symbol = compilation.GetTypeByMetadataName(metadataName);
        return symbol is null ? throw new InvalidOperationException($"Type {metadataName} is not found in compilation.") : symbol;
    }
}
