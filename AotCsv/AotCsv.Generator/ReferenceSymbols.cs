using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

// 参考：MemoryPack https://github.com/Cysharp/MemoryPack/blob/main/src/MemoryPack.Generator/ReferenceSymbols.cs

namespace Oucc.AotCsv.Generator;

internal class ReferenceSymbols
{
    public Compilation Compilation { get; }

    internal INamedTypeSymbol ISpanFormattable { get; private set; }
    internal INamedTypeSymbol IFormattable { get; private set; }
    internal INamedTypeSymbol Nullable_T { get; private set; }
    internal INamedTypeSymbol DateTime { get; private set; }
    internal INamedTypeSymbol String { get; private set; }
    internal INamedTypeSymbol CsvDateTimeFormatAttribute { get; private set; }

    internal ReferenceSymbols(Compilation compilation)
    {
        Compilation = compilation;
        ISpanFormattable = GetTypeByMetadataName("System.ISpanFormattable");
        IFormattable = GetTypeByMetadataName("System.IFormattable");
        Nullable_T = GetTypeByMetadataName("System.Nullable`1").ConstructUnboundGenericType();
        DateTime = GetTypeByMetadataName("System.DateTime");
        String = GetTypeByMetadataName("System.String");
        CsvDateTimeFormatAttribute = GetTypeByMetadataName("Oucc.AotCsv.Attributes.CsvDateTimeFormatAttribute");
    }

    INamedTypeSymbol GetTypeByMetadataName(string metadataName)
    {
        var symbol = Compilation.GetTypeByMetadataName(metadataName);
        if (symbol == null)
        {
            throw new InvalidOperationException($"Type {metadataName} is not found in compilation.");
        }
        return symbol;
    }
}
