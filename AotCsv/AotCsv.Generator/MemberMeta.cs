using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Oucc.AotCsv.Generator;

internal class MemberMeta
{
    public MemberMeta(IFieldSymbol fieldSymbol) : this(fieldSymbol, false, fieldSymbol.Type) { }

    public MemberMeta(IPropertySymbol propertySymbol) : this(propertySymbol, true, propertySymbol.Type) { }

    private MemberMeta(ISymbol symbol, bool isProperty, ITypeSymbol typeSymbol)
    {
        Debug.Assert(symbol is IFieldSymbol or IPropertySymbol);
        Symbol = symbol;
        IsProperty = isProperty;
        Type = typeSymbol;

        var nameAttribute = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToString() == "Oucc.AotCsv.Attributes.CsvNameAttribute");
        HeaderName = nameAttribute?.ConstructorArguments[0].Value as string ?? symbol.Name.Replace("\"", "\"\"");
    }

    public ISymbol Symbol { get; }

    public bool IsProperty { get; }

    public ITypeSymbol Type { get; }

    public string HeaderName { get; }
}
