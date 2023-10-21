using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

internal static class DeserializeCodeGenerator
{
    #region Header
    public static void WriteHeaderCode(StringBuilder builder, MemberMeta[] targetMembers, string targetTypeName, string helperClassName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        targetMembers = targetMembers.Where(m => (m.State & MemberMeta.MemberState.Settable) == MemberMeta.MemberState.Settable).ToArray();

        builder.AppendFormatted($$"""
            
                static void global::Oucc.AotCsv.ICsvSerializable<{{targetTypeName}}>.ParseHeader(global::Oucc.AotCsv.GeneratorHelpers.CsvParser parser, out global::System.Collections.Immutable.ImmutableArray<int> columnMap)
                {
                    if (!parser.Config.HasHeader)
                    {
                        columnMap = global::System.Collections.Immutable.ImmutableArray.Create<int>(
            """);

        for (var i = 0; i < targetMembers.Length; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(targetMembers[i].InternalId);
        }

        builder.AppendFormatted($$"""
            );
                        return;
                    }

                    global::System.Collections.Generic.List<int> rawColumnMap = new global::System.Collections.Generic.List<int>({{targetMembers.Length}});
                    int readValidColumns = 0;

                    while (true)
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
                        global::Oucc.AotCsv.Exceptions.CsvInvalidHeaderException.Throw(rawColumnMap, {{helperClassName}}.MappingMetadata);
                        columnMap = default;
                        return;
                    }

                    columnMap = global::System.Collections.Immutable.ImmutableArray.Create(global::System.Runtime.InteropServices.CollectionsMarshal.AsSpan(rawColumnMap));
                }

            """);
    }
    #endregion

    #region Body
    public static void WriteBodyCode(StringBuilder builder, MemberMeta[] targetMembers, ITypeSymbol targetType, ReferenceSymbols referenceSymbols, string helperClassName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        targetMembers = targetMembers.Where(m => (m.State & MemberMeta.MemberState.Settable) == MemberMeta.MemberState.Settable).ToArray();

        builder.AppendFormatted($$"""
            
                static bool global::Oucc.AotCsv.ICsvSerializable<{{targetType.ToDisplayString(SymbolFormat.NameOnly)}}>.ParseRecord(global::Oucc.AotCsv.GeneratorHelpers.CsvParser parser, [global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out {{targetType.ToDisplayString(SymbolFormat.NameOnly)}} value)
                {

            """);

        foreach (var member in targetMembers)
        {
            builder.AppendFormatted($$"""
                        {{member.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} @{{member.Symbol.Name}} = default!;

                """);
        }

        builder.AppendFormatted($$"""
                            
                    for (int columnIndex = 0; true; columnIndex++)
                    {
                        int targetIndex = parser.ColumnMap[columnIndex];
                        using global::Oucc.AotCsv.GeneratorHelpers.ArrayContainer container = parser.TryGetField(out global::Oucc.AotCsv.GeneratorHelpers.FieldState state);
                        global::System.ReadOnlySpan<char> field = container.AsSpan();

                        if (state == global::Oucc.AotCsv.GeneratorHelpers.FieldState.NoLine)
                        {
                            value = default;
                            return false;
                        }

                        switch (targetIndex)
                        {
            
            """);

        foreach (var member in targetMembers)
        {
            if (member.Type.Equals(referenceSymbols.String, SymbolEqualityComparer.Default))
                WriteParseString(builder, member);
            else if (member.TypeWithoutNullable.Equals(referenceSymbols.DateTime, SymbolEqualityComparer.Default))
                WriteDateTimeParseExact(builder, member, helperClassName);
            else if (member.TypeWithoutNullable.Equals(referenceSymbols.Boolean, SymbolEqualityComparer.Default))
                WriteParseBool(builder, member, helperClassName);
            else
                WriteSpanParsable(builder, member, helperClassName);
        }

        builder.AppendFormatted($$"""
                        }

                        if (state == global::Oucc.AotCsv.GeneratorHelpers.FieldState.LastField)
                        {
                            if (columnIndex < parser.ColumnMap.Length - 1)
                                global::Oucc.AotCsv.Exceptions.TooFewColumnsException.Throw({{helperClassName}}.MappingMetadata, columnIndex + 1, parser.ColumnMap.Length);
                            break;
                        }
                    }

                    value = new {{targetType.ToDisplayString(SymbolFormat.NameOnly)}}()
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

    private static void WriteDateTimeParseExact(StringBuilder builder, MemberMeta memberMeta, string helperClassName)
    {
        var (format, styles) = memberMeta.DateTimeFormat;
        if (memberMeta.IsNullableStruct)
            builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                {
                                    if (field.IsEmpty) @{{memberMeta.Symbol.Name}} = null;
                                    else if (!global::System.DateTime.TryParseExact(field, @"{{format}}", parser.Config.CultureInfo, (global::System.Globalization.DateTimeStyles){{(int)styles}}, out global::System.DateTime temp))
                                    {
                                        global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{helperClassName}}.MappingMetadata);
                                        value = default;
                                        return false;
                                    }
                                    else @{{memberMeta.Symbol.Name}} = temp;
                                    break;
                                }

            """);
        else
            builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                if (!global::System.DateTime.TryParseExact(field, @"{{format}}", parser.Config.CultureInfo, (global::System.Globalization.DateTimeStyles){{(int)styles}}, out @{{memberMeta.Symbol.Name}}))
                                {
                                    global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{helperClassName}}.MappingMetadata);
                                    value = default;
                                    return false;
                                }
                                break;

            """);
    }

    private static void WriteParseBool(StringBuilder builder, MemberMeta memberMeta, string helperClassName)
    {
        if (memberMeta.IsNullableStruct)
            builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                {
                                    if (field.IsEmpty) @{{memberMeta.Symbol.Name}} = null;
                                    else if (!bool.TryParse(field, out bool temp))
                                    {
                                        global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{helperClassName}}.MappingMetadata);
                                        value = default;
                                        return false;
                                    }
                                    else @{{memberMeta.Symbol.Name}} = temp;
                                    break;
                                }

            """);
        else
            builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                if (!bool.TryParse(field, out @{{memberMeta.Symbol.Name}}))
                                {
                                    global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{helperClassName}}.MappingMetadata);
                                    value = default;
                                    return false;
                                }
                                break;

            """);
    }

    private static void WriteSpanParsable(StringBuilder builder, MemberMeta memberMeta, string helperClassName)
    {
        if (memberMeta.IsNullableStruct)
            builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                {
                                    if (field.IsEmpty) @{{memberMeta.Symbol.Name}} = null;
                                    else if (!global::Oucc.AotCsv.GeneratorHelpers.StaticInterfaceHelper.TryParse<{{memberMeta.TypeWithoutNullable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}>(field, parser.Config.CultureInfo, out {{memberMeta.TypeWithoutNullable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}} temp))
                                    {
                                        global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{helperClassName}}.MappingMetadata);
                                        value = default;
                                        return false;
                                    }
                                    else @{{memberMeta.Symbol.Name}} = temp;
                                    break;
                                }

            """);
        else
            builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                if (!global::Oucc.AotCsv.GeneratorHelpers.StaticInterfaceHelper.TryParse<{{memberMeta.TypeWithoutNullable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}>(field, parser.Config.CultureInfo, out @{{memberMeta.Symbol.Name}}))
                                {
                                    global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{helperClassName}}.MappingMetadata);
                                    value = default;
                                    return false;
                                }
                                break;

            """);
    }
    #endregion
}
