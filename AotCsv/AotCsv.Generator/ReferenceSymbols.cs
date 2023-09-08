using System;
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
    internal INamedTypeSymbol CsvDateTimeFormatAttribute { get; }
    internal INamedTypeSymbol CsvIncludeAttribute { get; }
    internal INamedTypeSymbol CsvIgnoreAttribute { get; }

    internal ReferenceSymbols(Compilation compilation)
    {
        Compilation = compilation;
        ISpanFormattable = GetTypeByMetadataName(compilation, "System.ISpanFormattable");
        IFormattable = GetTypeByMetadataName(compilation, "System.IFormattable");
        Nullable_T = GetTypeByMetadataName(compilation, "System.Nullable`1").ConstructUnboundGenericType();
        DateTime = GetTypeByMetadataName(compilation, "System.DateTime");
        String = GetTypeByMetadataName(compilation, "System.String");
        CsvDateTimeFormatAttribute = GetTypeByMetadataName(compilation, "Oucc.AotCsv.Attributes.CsvDateTimeFormatAttribute");
        CsvIncludeAttribute = GetTypeByMetadataName(compilation, "Oucc.AotCsv.Attributes.CsvIncludeAttribute");
        CsvIgnoreAttribute = GetTypeByMetadataName(compilation, "Oucc.AotCsv.Attributes.CsvIgnoreAttribute");
    }

    private static INamedTypeSymbol GetTypeByMetadataName(Compilation compilation, string metadataName)
    {
        var symbol = compilation.GetTypeByMetadataName(metadataName);
        if (symbol == null)
        {
            throw new InvalidOperationException($"Type {metadataName} is not found in compilation.");
        }
        return symbol;
    }
}
