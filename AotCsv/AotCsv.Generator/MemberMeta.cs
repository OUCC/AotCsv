using System.Diagnostics;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

internal class MemberMeta
{
    public MemberMeta(int id, IFieldSymbol fieldSymbol) : this(id, fieldSymbol, MemberState.FullAccessable, fieldSymbol.Type) { }

    public MemberMeta(int id, IPropertySymbol propertySymbol) : this(
        id,
        propertySymbol,
        MemberState.Property | (propertySymbol.IsReadOnly ? MemberState.Readable : propertySymbol.IsWriteOnly ? MemberState.Settable : MemberState.FullAccessable),
        propertySymbol.Type)
    { }

    private MemberMeta(int id, ISymbol symbol, MemberState MemberState, ITypeSymbol typeSymbol)
    {
        Debug.Assert(symbol is IFieldSymbol or IPropertySymbol);
        InternalId = id;
        Symbol = symbol;
        State = MemberState;
        Type = typeSymbol;
        TypeWithoutNullable = typeSymbol.EqualsByMetadataName(new[] { "System", "Nullable`1" }) ? ((INamedTypeSymbol)typeSymbol).TypeArguments[0] : typeSymbol;

        var attributes = symbol.GetAttributes();
        var nameAttribute = attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvNameAttribute" }));
        HeaderName = nameAttribute?.ConstructorArguments[0].Value as string ?? symbol.Name.Replace("\"", "\"\"");
        Index = attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvIndexAttribute" }))?.ConstructorArguments[0].Value as uint?;
        Format = (attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvFormatAttribute" }))?.ConstructorArguments[0].Value as string)?.Replace("\"", "\"\"");
        var datetTimeAttribute = attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvDateTimeFormatAttribute" }));
        DateTimeFormat = ((datetTimeAttribute?.ConstructorArguments[0].Value as string)?.Replace("\"", "\"\""), datetTimeAttribute?.ConstructorArguments[1].Value is DateTimeStyles styles ? styles : DateTimeStyles.None);
    }

    public int InternalId { get; }

    public ISymbol Symbol { get; }

    public MemberState State { get; }

    public ITypeSymbol Type { get; }

    public ITypeSymbol TypeWithoutNullable { get; }

    /// <summary>
    /// Headerの名前 エスケープ済み
    /// </summary>
    public string HeaderName { get; }

    public uint? Index { get; }

    /// <summary>
    /// CsvFormatAttribute の値 エスケープ済み
    /// </summary>
    public string? Format { get; }

    /// <summary>
    /// CsvDateTimeFormatAttribute の値 エスケープ済み
    /// </summary>
    public (string?, DateTimeStyles) DateTimeFormat { get; }


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
