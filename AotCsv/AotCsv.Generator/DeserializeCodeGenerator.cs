using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

internal static class DeserializeCodeGenerator
{
    #region Header
    public static void WriteHeaderCode(StringBuilder builder, MemberMeta[] targetMembers, string targetTypeName)
    {
        targetMembers = targetMembers.Where(m => (m.State & MemberMeta.MemberState.Settable) == MemberMeta.MemberState.Settable).ToArray();

        Debug.Assert(targetMembers.Length > 0);
        builder.AppendFormatted($$"""
            
                static void global::Oucc.AotCsv.ICsvSerializable<{{targetTypeName}}>.ParseHeader(global::Oucc.AotCsv.GeneratorHelpers.CsvParser parser, out global::System.Collections.Immutable.ImmutableArray<int> columnMap)
                {
                    if (!parser.Config.HasHeader)
                    {
                        columnMap = global::System.Collections.Immutable.ImmutableArray.Create<int>(
            """);

        for (var i = 1; i < targetMembers.Length; i++)
        {
            builder.Append(i);
            builder.Append(", ");
        }
        builder.Append(targetMembers.Length);

        builder.AppendFormatted($$"""
            );
                        return;
                    }

                    global::System.Collections.Generic.List<int> rawColumnMap = new global::System.Collections.Generic.List<int>({{targetMembers.Length}});
                    int readValidColumns = 0;

                    while (readValidColumns < {{targetMembers.Length}})
                    {
                        using global::Oucc.AotCsv.GeneratorHelpers.ArrayContainer field = parser.TryGetField(out global::Oucc.AotCsv.GeneratorHelpers.FieldState state);
                        if (state == global::Oucc.AotCsv.GeneratorHelpers.FieldState.NoLine)
                        {
                            global::Oucc.AotCsv.AotCsvException.ThrowBaseException();
                            columnMap = default;
                            return;
                        }

                        int value = field.AsSpan() switch
                        {

            """);


        foreach (var member in targetMembers)
        {
            builder.AppendFormatted($$"""
                                @"{{member.HeaderName}}" => {{member.InternalId}},
                
                """);
        }

        builder.AppendFormatted($$"""
                            _ => 0,
                        };
                        rawColumnMap.Add(value);
                        if (value > 0)
                            readValidColumns++;

                        if (state == global::Oucc.AotCsv.GeneratorHelpers.FieldState.LastField)
                        {
                            break;
                        }
                    }

                    if (readValidColumns < {{targetMembers.Length}})
                    {
                        global::Oucc.AotCsv.Exceptions.CsvInvalidHeaderException.Throw(rawColumnMap, {{targetMembers[0].Symbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}.Helper.MappingMetadata);
                        columnMap = default;
                        return;
                    }

                    columnMap = global::System.Collections.Immutable.ImmutableArray.Create(global::System.Runtime.InteropServices.CollectionsMarshal.AsSpan(rawColumnMap));
                }

            """);
    }
    #endregion

    #region Body
    public static void WriteBodyCode(StringBuilder builder, MemberMeta[] targetMembers, ITypeSymbol targetType, ReferenceSymbols referenceSymbols)
    {
        targetMembers = targetMembers.Where(m => (m.State & MemberMeta.MemberState.Settable) == MemberMeta.MemberState.Settable).ToArray();

        builder.AppendFormatted($$"""
            
                static bool global::Oucc.AotCsv.ICsvSerializable<{{targetType.Name}}>.ParseRecord(global::Oucc.AotCsv.GeneratorHelpers.CsvParser parser, [global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out {{targetType.Name}} value)
                {

            """);

        foreach (var member in targetMembers)
        {
            builder.AppendFormatted($$"""
                        {{member.TypeWithoutNullable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} @{{member.Symbol.Name}} = default!;

                """);
        }

        builder.AppendFormatted($$"""
                            
                    for (int columnIndex = 0; columnIndex < parser.ColumnMap.Length; columnIndex++)
                    {
                        int targetIndex = parser.ColumnMap[columnIndex];
                        using global::Oucc.AotCsv.GeneratorHelpers.ArrayContainer container = parser.TryGetField(out global::Oucc.AotCsv.GeneratorHelpers.FieldState state);
                        global::System.ReadOnlySpan<char> field = container.AsSpan();

                        if (state == global::Oucc.AotCsv.GeneratorHelpers.FieldState.NoLine)
                        {
                            value = null;
                            return false;
                        }

                        switch (targetIndex)
                        {

            """);

        foreach (var member in targetMembers)
        {
            if (member.Type.Equals(referenceSymbols.String, SymbolEqualityComparer.Default))
                WriteParseString(builder, member);
            else if (member.Type.Equals(referenceSymbols.DateTime, SymbolEqualityComparer.Default))
                WriteDateTimeParseExact(builder, member);
            else if (member.TypeWithoutNullable.Equals(referenceSymbols.Boolean, SymbolEqualityComparer.Default))
                WriteParseBool(builder, member);
            else
                WriteSpanParsable(builder, member);
        }

        builder.AppendFormatted($$"""
                        }
                    }

                    value = new {{targetType.Name}}()
                    {

            """);

        foreach (var member in targetMembers)
        {
            builder.AppendFormatted($$"""
                            @{{member.Symbol.Name}} = @{{member.Symbol.Name}}!,

                """);
        }

        builder.Append("""
                    };
                    return true;
                }

            """);
    }

    private static void WriteParseString(StringBuilder builder, MemberMeta memberMeta)
    {
        builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                @{{memberMeta.Symbol.Name}} = field.ToString();
                                break;

            """);
    }

    private static void WriteDateTimeParseExact(StringBuilder builder, MemberMeta memberMeta)
    {
        var (format, styles) = memberMeta.DateTimeFormat;
        builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                if (!global::System.DateTime.TryParseExact(field, @"{{format}}", parser.Config.CultureInfo, (global::System.Globalization.DateTimeStyles){{(int)styles}}, out @{{memberMeta.Symbol.Name}}))
                                {
                                    global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{memberMeta.Symbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}.Helper.MappingMetadata);
                                    value = null;
                                    return false;
                                }
                                break;

            """);
    }

    private static void WriteParseBool(StringBuilder builder, MemberMeta memberMeta)
    {
        builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                if(!bool.TryParse(field, out @{{memberMeta.Symbol.Name}}))
                                {
                                    global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{memberMeta.Symbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}.Helper.MappingMetadata);
                                    value = null;
                                    return false;
                                }
                                break;

            """);
    }

    private static void WriteSpanParsable(StringBuilder builder, MemberMeta memberMeta)
    {
        builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                if (!global::Oucc.AotCsv.GeneratorHelpers.StaticInterfaceHelper.TryParse<{{memberMeta.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}>(field, parser.Config.CultureInfo, out @{{memberMeta.Symbol.Name}}))
                                {
                                    global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{memberMeta.Symbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}.Helper.MappingMetadata);
                                    value = null;
                                    return false;
                                }
                                break;

            """);
    }
    #endregion
}
