using System.Diagnostics;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

internal class MemberMeta
{
    public MemberMeta(int id, IFieldSymbol fieldSymbol) : this(id, fieldSymbol, MemberState.FullAccessible, fieldSymbol.Type) { }

    public MemberMeta(int id, IPropertySymbol propertySymbol) : this(
        id,
        propertySymbol,
        MemberState.Property | (propertySymbol.IsReadOnly ? MemberState.Readable : propertySymbol.IsWriteOnly ? MemberState.Settable : MemberState.FullAccessible),
        propertySymbol.Type)
    { }

    private MemberMeta(int id, ISymbol symbol, MemberState MemberState, ITypeSymbol typeSymbol)
    {
        Debug.Assert(symbol is IFieldSymbol or IPropertySymbol);
        InternalId = id;
        Symbol = symbol;
        State = MemberState;
        Type = typeSymbol;
        IsNullableStruct = typeSymbol.EqualsByMetadataName(new[] { "System", "Nullable`1" });
        InnerType = IsNullableStruct ? ((INamedTypeSymbol)typeSymbol).TypeArguments[0] : typeSymbol;
        IsTypeParameter = InnerType is ITypeParameterSymbol;

        SymbolName = symbol.ToDisplayString(SymbolFormat.NameOnly);
        TypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        InnerTypeName = InnerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var attributes = symbol.GetAttributes();
        var nameAttribute = attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvNameAttribute" }));
        HeaderName = nameAttribute?.ConstructorArguments[0].Value as string ?? symbol.Name.Replace("\"", "\"\"");
        Index = attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvIndexAttribute" }))?.ConstructorArguments[0].Value as uint?;
        Format = (attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvFormatAttribute" }))?.ConstructorArguments[0].Value as string)?.Replace("\"", "\"\"");
        var dateTimeAttribute = attributes.FirstOrDefault(a => a.AttributeClass.EqualsByMetadataName(new[] { "Oucc", "AotCsv", "Attributes", "CsvDateTimeFormatAttribute" }));
        DateTimeFormat = ((dateTimeAttribute?.ConstructorArguments[0].Value as string)?.Replace("\"", "\"\""), dateTimeAttribute?.ConstructorArguments[1].Value is DateTimeStyles styles ? styles : DateTimeStyles.None);
    }

    public int InternalId { get; }

    public ISymbol Symbol { get; }

    public MemberState State { get; }

    public ITypeSymbol Type { get; }

    /// <summary>
    /// Nullable&lt;T&gt; の T の型 そうでない場合は <see cref="Type"/>
    /// </summary>
    public ITypeSymbol InnerType { get; }

    public string SymbolName { get; }

    public string TypeName { get; }

    public string InnerTypeName { get; }

    public bool IsNullableStruct { get; }

    public bool IsTypeParameter { get; }

    /// <summary>
    /// Headerの名前 文字列リテラル用エスケープ済み
    /// </summary>
    public string HeaderName { get; }

    public uint? Index { get; }

    /// <summary>
    /// CsvFormatAttribute の値 文字列リテラル用エスケープ済み
    /// </summary>
    public string? Format { get; }

    /// <summary>
    /// CsvDateTimeFormatAttribute の値 文字列リテラル用エスケープ済み
    /// </summary>
    public (string?, DateTimeStyles) DateTimeFormat { get; }


    [Flags]
    public enum MemberState
    {
        None,
        Property = 1,
        Settable = 2,
        Readable = 4,
        FullAccessible = Settable | Readable,
    }
}
