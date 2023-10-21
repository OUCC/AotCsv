using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

internal static class DeserializeCodeGenerator
{
    #region Header
    public static void WriteHeaderCode(StringBuilder builder, TargetTypeMeta targetType, MemberMeta[] targetMembers, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        targetMembers = targetMembers.Where(m => (m.State & MemberMeta.MemberState.Settable) == MemberMeta.MemberState.Settable).ToArray();

        builder.AppendFormatted($$"""
            
                static void global::Oucc.AotCsv.ICsvSerializable<{{targetType.Name}}>.ParseHeader(global::Oucc.AotCsv.GeneratorHelpers.CsvParser parser, out global::System.Collections.Immutable.ImmutableArray<int> columnMap)
                {
                    if (!parser.Config.HasHeader)
                    {
                        columnMap = global::System.Collections.Immutable.ImmutableArray.Create<int>(
            """);

        for (var i = 1; i < targetMembers.Length; i++)
        {
            builder.Append(targetMembers[i].InternalId);
            builder.Append(", ");
        }
        builder.Append(targetMembers.Length);

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
                        global::Oucc.AotCsv.Exceptions.CsvInvalidHeaderException.Throw(rawColumnMap, {{targetType.HelperClassFullName}}.MappingMetadata);
                        columnMap = default;
                        return;
                    }

                    columnMap = global::System.Collections.Immutable.ImmutableArray.Create(global::System.Runtime.InteropServices.CollectionsMarshal.AsSpan(rawColumnMap));
                }

            """);
    }
    #endregion

    #region Body
    public static void WriteBodyCode(StringBuilder builder, TargetTypeMeta targetType, MemberMeta[] targetMembers, ReferenceSymbols referenceSymbols, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        targetMembers = targetMembers.Where(m => (m.State & MemberMeta.MemberState.Settable) == MemberMeta.MemberState.Settable).ToArray();

        builder.AppendFormatted($$"""
            
                static bool global::Oucc.AotCsv.ICsvSerializable<{{targetType.Name}}>.ParseRecord(global::Oucc.AotCsv.GeneratorHelpers.CsvParser parser, [global::System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out {{targetType.Name}} value)
                {

            """);

        foreach (var member in targetMembers)
        {
            builder.AppendFormatted($$"""
                        {{member.TypeName}} {{member.SymbolName}} = default!;

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
            else if (member.InnerType.Equals(referenceSymbols.DateTime, SymbolEqualityComparer.Default))
                WriteDateTimeParseExact(builder, member, targetType.HelperClassFullName);
            else if (member.InnerType.Equals(referenceSymbols.Boolean, SymbolEqualityComparer.Default))
                WriteParseBool(builder, member, targetType.HelperClassFullName);
            else
                WriteSpanParsable(builder, member, targetType.HelperClassFullName);
        }

        builder.AppendFormatted($$"""
                        }

                        if (state == global::Oucc.AotCsv.GeneratorHelpers.FieldState.LastField)
                        {
                            if (columnIndex < parser.ColumnMap.Length - 1)
                                global::Oucc.AotCsv.Exceptions.TooFewColumnsException.Throw({{targetType.HelperClassFullName}}.MappingMetadata, columnIndex + 1, parser.ColumnMap.Length);
                            break;
                        }
                    }

                    value = new {{targetType.Name}}()
                    {

            """);

        foreach (var member in targetMembers)
        {
            builder.AppendFormatted($$"""
                            {{member.SymbolName}} = {{member.SymbolName}}!,

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
                                {{memberMeta.SymbolName}} = field.ToString();
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
                                    if (field.IsEmpty) {{memberMeta.SymbolName}} = null;
                                    else if (!global::System.DateTime.TryParseExact(field, @"{{format}}", parser.Config.CultureInfo, (global::System.Globalization.DateTimeStyles){{(int)styles}}, out global::System.DateTime temp))
                                    {
                                        global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{helperClassName}}.MappingMetadata);
                                        value = default;
                                        return false;
                                    }
                                    else {{memberMeta.SymbolName}} = temp;
                                    break;
                                }

            """);
        else
            builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                if (!global::System.DateTime.TryParseExact(field, @"{{format}}", parser.Config.CultureInfo, (global::System.Globalization.DateTimeStyles){{(int)styles}}, out {{memberMeta.SymbolName}}))
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
                                    if (field.IsEmpty) {{memberMeta.SymbolName}} = null;
                                    else if (!bool.TryParse(field, out bool temp))
                                    {
                                        global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{helperClassName}}.MappingMetadata);
                                        value = default;
                                        return false;
                                    }
                                    else {{memberMeta.SymbolName}} = temp;
                                    break;
                                }
                
            """);
        else
            builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                if (!bool.TryParse(field, out {{memberMeta.SymbolName}}))
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
                                    if (field.IsEmpty) {{memberMeta.SymbolName}} = null;
                                    else if (!global::Oucc.AotCsv.GeneratorHelpers.StaticInterfaceHelper.TryParse<{{memberMeta.InnerTypeName}}>(field, parser.Config.CultureInfo, out {{memberMeta.InnerTypeName}} temp))
                                    {
                                        global::Oucc.AotCsv.Exceptions.CsvTypeConvertException.Throw(field.ToString(), {{memberMeta.InternalId}}, {{helperClassName}}.MappingMetadata);
                                        value = default;
                                        return false;
                                    }
                                    else {{memberMeta.SymbolName}} = temp;
                                    break;
                                }
                
            """);
        else
            builder.AppendFormatted($$"""
                            case {{memberMeta.InternalId}}:
                                if (!global::Oucc.AotCsv.GeneratorHelpers.StaticInterfaceHelper.TryParse<{{memberMeta.InnerTypeName}}>(field, parser.Config.CultureInfo, out {{memberMeta.SymbolName}}))
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
