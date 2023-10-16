using System.Diagnostics;
using System.Globalization;
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


        for (var i = 0; i < targetMembers.Length;)
        {
            builder.AppendFormatted($$"""
                                @"{{targetMembers[i].HeaderName.Replace("\"", "\"\"")}}" => {{++i}},
                
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
                        global::Oucc.AotCsv.Exceptions.CsvInvalidHeaderException.Throw(rawColumnMap);
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
            
                static bool global::Oucc.AotCsv.ICsvSerializable<{{targetType.Name}}>.ParseRecord(global::Oucc.AotCsv.GeneratorHelpers.CsvParser parser, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out {{targetType.Name}}? value)
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

        for (var i = 0; i < targetMembers.Length; i++)
        {
            if (targetMembers[i].Type.Equals(referenceSymbols.String, SymbolEqualityComparer.Default))
                WriteParseString(builder, i + 1, targetMembers[i].Symbol.Name);
            else if (targetMembers[i].Type.Equals(referenceSymbols.DateTime, SymbolEqualityComparer.Default))
            {
                var formatAttribute = targetMembers[i].Symbol.GetAttributes().Where(x => x.AttributeClass!.Equals(referenceSymbols.CsvDateTimeFormatAttribute, SymbolEqualityComparer.Default)).First();
                var format = string.Concat("\"", (formatAttribute.ConstructorArguments[0].Value as string)?.Replace("\"", "\"\""), "\"") ?? "default";
                var dateTimeStyles = formatAttribute.ConstructorArguments[1].Value is DateTimeStyles styles ? styles : DateTimeStyles.None;
                WriteDateTimeParseExact(builder, i + 1, targetMembers[i].Symbol.Name, format, dateTimeStyles);
            }
            else if (targetMembers[i].TypeWithoutNullable.Equals(referenceSymbols.Boolean, SymbolEqualityComparer.Default))
                WriteParseBool(builder, i + 1, targetMembers[i].Symbol.Name);
            else
                WriteSpanParsable(builder, i + 1, targetMembers[i].Symbol.Name, targetMembers[i].TypeWithoutNullable.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
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
                            {{member.Symbol.Name}} = @{{member.Symbol.Name}}!,

                """);
        }

        builder.Append("""
                    };
                    return true;
                }

            """);
    }

    private static void WriteParseString(StringBuilder builder, int index, string variable)
    {
        builder.AppendFormatted($$"""
                            case {{index}}:
                                @{{variable}} = field.ToString();
                                break;

            """);
    }

    private static void WriteDateTimeParseExact(StringBuilder builder, int index, string variable, string format, DateTimeStyles dateTimeStyle)
    {
        builder.AppendFormatted($$"""
                            case {{index}}:
                                if (!global::System.DateTime.TryParseExact(field, {{format}}, parser.Config.CultureInfo, (global::System.Globalization.DateTimeStyles){{(int)dateTimeStyle}}, out @{{variable}}))
                                {
                                    global::Oucc.AotCsv.Exceptions.CsvTypeException.Throw(typeof(global::System.DateTime), field.ToString(), @"{{variable}}");
                                    value = null;
                                    return false;
                                }
                                break;

            """);
    }

    private static void WriteParseBool(StringBuilder builder, int index, string variable)
    {
        builder.AppendFormatted($$"""
                            case {{index}}:
                                if(!bool.TryParse(field, out @{{variable}}))
                                {
                                    global::Oucc.AotCsv.Exceptions.CsvTypeException.Throw(typeof(bool), field.ToString(), @"{{variable}}");
                                    value = null;
                                    return false;
                                }
                                break;

            """);
    }

    private static void WriteSpanParsable(StringBuilder builder, int index, string variable, string typeFullName)
    {
        builder.AppendFormatted($$"""
                            case {{index}}:
                                if (!global::Oucc.AotCsv.GeneratorHelpers.StaticInterfaceHelper.TryParse<{{typeFullName}}>(field, parser.Config.CultureInfo, out @{{variable}}))
                                {
                                    global::Oucc.AotCsv.Exceptions.CsvTypeException.Throw(typeof({{typeFullName}}), field.ToString(), @"{{variable}}");
                                    value = null;
                                    return false;
                                }
                                break;

            """);
    }
    #endregion
}
