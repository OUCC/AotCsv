using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

internal class MemberMeta
{
    public MemberMeta(IFieldSymbol fieldSymbol) : this(fieldSymbol, MemberState.FullAccessable, fieldSymbol.Type) { }

    public MemberMeta(IPropertySymbol propertySymbol) : this(
        propertySymbol,
        MemberState.Property | (propertySymbol.IsReadOnly ? MemberState.Readable : propertySymbol.IsWriteOnly ? MemberState.Settable : MemberState.FullAccessable),
        propertySymbol.Type)
    { }

    private MemberMeta(ISymbol symbol, MemberState MemberState, ITypeSymbol typeSymbol)
    {
        Debug.Assert(symbol is IFieldSymbol or IPropertySymbol);
        Symbol = symbol;
        State = MemberState;
        Type = typeSymbol;
        TypeWithoutNullable = typeSymbol.EqualsByMetadataName(new[] { "System", "Nullable`1" }) ? ((INamedTypeSymbol)typeSymbol).TypeArguments[0] : typeSymbol;

        var attributes = symbol.GetAttributes();
        var nameAttribute = attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvNameAttribute" }));
        HeaderName = nameAttribute?.ConstructorArguments[0].Value as string ?? symbol.Name.Replace("\"", "\"\"");
        Index = attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvIndexAttribute" }))?.ConstructorArguments[0].Value as uint?;
        Format = attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvFormatAttribute" }))?.ConstructorArguments[0].Value as string;
    }

    public ISymbol Symbol { get; }

    public MemberState State { get; }

    public ITypeSymbol Type { get; }

    public ITypeSymbol TypeWithoutNullable { get; }

    public string HeaderName { get; }

    public uint? Index { get; }

    public string? Format { get; }

    [Flags]
    public enum MemberState
    {
        None,
        Property = 1,
        Settable = 2,
        Readable = 4,
        FullAccessable = Settable | Readable,
    }
}
