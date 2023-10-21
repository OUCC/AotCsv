using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

internal static class SerializeCodeGenerator
{
    #region Header
    public static void CreateHeaderCode(MemberMeta[] targetMembers, StringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        builder.AppendFormatted($$"""
                    if (context.QuoteOption == {{Constants.QuoteOption}}.MustQuote)
                    {
                        writer.Write($@"
            """);

        for (var i = 0; i < targetMembers.Length; i++)
        {
            builder.AppendFormatted($"\"\"{targetMembers[i].HeaderName.Replace("\"\"","\"\"\"\"")}\"\"");
            if (i < targetMembers.Length - 1)
                builder.Append(',');
        }

        builder.AppendFormatted($$"""
            {context.NewLine}");
                    }
                    else if (context.QuoteOption == {{Constants.QuoteOption}}.ShouldQuote)
                    {
                        writer.Write($@"
            """);

        var canMustNoQuote = true;
        for (var i = 0; i < targetMembers.Length; i++)
        {
            if (targetMembers[i].HeaderName.AsSpan().IndexOfAny("\",\r\n".AsSpan()) >= 0)
            {
                builder.AppendFormatted($"\"\"{targetMembers[i].HeaderName.Replace("\"\"", "\"\"\"\"")}\"\"");
                canMustNoQuote = false;
            }
            else builder.Append(targetMembers[i].HeaderName);

            if (i < targetMembers.Length - 1)
                builder.Append(',');
        }

        builder.AppendFormatted($$"""
            {context.NewLine}");
                    }
                    else
                    {

            """);

        if(canMustNoQuote)
        {
            builder.Append("            writer.Write($\"");
            for (var i = 0; i < targetMembers.Length; i++)
            {
                builder.Append(targetMembers[i].HeaderName);

                if (i < targetMembers.Length - 1)
                    builder.Append(',');
            }
            builder.Append("{context.NewLine}\");\n");
        }
        else
        {
            builder.Append("            global::Oucc.AotCsv.AotCsvException.ThrowMustNoQuoteException();\n");
        }

        builder.Append("        }\n");
    }
    #endregion 

    #region Body
    public static void CreateBodyCode(StringBuilder builder, MemberMeta[] targetMembers, ReferenceSymbols reference, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        builder.Append("""

                    char[] buffer = global::System.Buffers.ArrayPool<char>.Shared.Rent(1024);
                    global::System.Span<char> bufferSpan = buffer.AsSpan()[..1024];
                    int charsWritten = 0;
                    try
                    {
            """);

        for (var i = 0; i < targetMembers.Length; i++)
        {
            var member = targetMembers[i];
            var typeSymbol = member.Type;

            // string(?)の時
            if (member.Type.Equals(reference.String, SymbolEqualityComparer.Default))
            {
                NullableStringSerializeCodegen(builder, member.SymbolName);
            }
            // struct?のとき
            else if (member.IsNullableStruct)
            {
                var typeParameter = member.InnerType;
                if (typeParameter.Equals(reference.DateTime, SymbolEqualityComparer.IncludeNullability))
                {
                    DateTimeSerializeCodegen(builder, member, reference, true);
                }
                else if (typeParameter.Equals(reference.Boolean, SymbolEqualityComparer.IncludeNullability))
                {
                    NullableBooleanSerializeCodegen(builder, member.SymbolName);
                }
                else if (typeParameter.Equals(reference.Char, SymbolEqualityComparer.IncludeNullability))
                {
                    NullableStructISpanFormattableSerializeCodegen(builder, member.SymbolName, true);
                }
                else
                {
                    NullableStructISpanFormattableSerializeCodegen(builder, member.SymbolName);
                }
            }
            // class? と struct制約のない型パラメータ
            else if (typeSymbol.NullableAnnotation != NullableAnnotation.NotAnnotated
                     && typeSymbol.IsReferenceType
                     || typeSymbol is ITypeParameterSymbol parameterSymbol && !parameterSymbol.HasValueTypeConstraint && !parameterSymbol.HasReferenceTypeConstraint)
            {
                NullableClassISpanFormattableSerializeCodegen(builder, member.SymbolName);
            }
            else
            {
                if (typeSymbol.Equals(reference.DateTime, SymbolEqualityComparer.IncludeNullability))
                {
                    DateTimeSerializeCodegen(builder, member, reference, false);
                }
                else if (typeSymbol.Equals(reference.Boolean, SymbolEqualityComparer.IncludeNullability))
                {
                    BooleanSerializeCodegen(builder, member.SymbolName);
                }
                else if (typeSymbol.Equals(reference.Char, SymbolEqualityComparer.IncludeNullability))
                {
                    ISpanFormattableSerializeCodegen(builder, member.SymbolName, true);
                }
                else
                {
                    ISpanFormattableSerializeCodegen(builder, member.SymbolName);
                }
            }

            if (i < targetMembers.Length - 1)
            {
                builder.Append("""
                            writer.Write(',');

                """);
            }
        }

        builder.Append("""

                        writer.Write(config.NewLine);
                    }
                    finally
                    {
                        global::System.Buffers.ArrayPool<char>.Shared.Return(buffer);
                    }

            """);
    }

    #region Each Types Generator
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NullableBooleanSerializeCodegen(StringBuilder builder, string name)
    {
        builder.AppendFormatted($$"""
    
                        if (value.{{name}} is not null)
                        {
                            if (value.{{name}}.Value.TryFormat(bufferSpan, out charsWritten))
                            {
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
                            }
                        }
                        else
                        {
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, default, config, 0);
                        }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BooleanSerializeCodegen(StringBuilder builder, string name)
    {
        builder.AppendFormatted($$"""

                        if (value.{{name}}.TryFormat(bufferSpan, out charsWritten))
                        {
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
                        }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DateTimeSerializeCodegen(StringBuilder builder, MemberMeta memberMeta, ReferenceSymbols reference, bool nullable)
    {
        var attributes = memberMeta.Symbol.GetAttributes();
        if (attributes.IsEmpty)
        {
            if (nullable)
            {
                NullableStructISpanFormattableSerializeCodegen(builder, memberMeta.SymbolName);
            }
            else ISpanFormattableSerializeCodegen(builder, memberMeta.SymbolName);
        }
        else
        {
            var formatAttribute = attributes.Where(x => x.AttributeClass!.Equals(reference.CsvDateTimeFormatAttribute, SymbolEqualityComparer.IncludeNullability)).First();
            if (formatAttribute != null)
            {
                string format = (string?)formatAttribute.ConstructorArguments[0].Value ?? "";
                if (nullable)
                {
                    NullableDateTimeSerializeCodegenWithFormat(builder, memberMeta.SymbolName, format);
                }
                else
                {
                    DateTimeSerializeCodegenWithFormat(builder, memberMeta.SymbolName, format);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DateTimeSerializeCodegenWithFormat(StringBuilder builder, string name, string format)
    {
        builder.AppendFormatted($$"""

                        if (value.{{name}}.TryFormat(bufferSpan, out charsWritten, "{{format}}", config.CultureInfo))
                        {
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
                        }
                        else
                        {
                            char[] tmp = global::System.Buffers.ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                            while (!value.{{name}}.TryFormat(tmp, out charsWritten, "{{format}}", config.CultureInfo))
                            {
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.EnsureBuffer(ref tmp);
                            }
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                            global::System.Buffers.ArrayPool<char>.Shared.Return(tmp);
                        }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NullableDateTimeSerializeCodegenWithFormat(StringBuilder builder, string name, string format)
    {
        builder.AppendFormatted($$"""

                        if (value.{{name}} is not null)
                        {
                            if (value.{{name}}.Value.TryFormat(bufferSpan, out charsWritten, "{{format}}", config.CultureInfo))
                            {
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
                            }
                            else
                            {
                                char[] tmp = global::System.Buffers.ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                                while (!value.{{name}}.Value.TryFormat(tmp, out charsWritten, "{{format}}", config.CultureInfo))
                                {
                                    global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.EnsureBuffer(ref tmp);
                                }
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                                global::System.Buffers.ArrayPool<char>.Shared.Return(tmp);
                            }
                        }
                        else
                        {
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, default, config, 0);
                        }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ISpanFormattableSerializeCodegen(StringBuilder builder, string name, bool isChar = false)
    {
        builder.AppendFormatted($$"""

                        if ({{(isChar ? $"((ISpanFormattable)value.{name})" : $"value.{name}")}}.TryFormat(bufferSpan, out charsWritten, default, config.CultureInfo))
                        {
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
                        }
                        else
                        {
                            char[] tmp = global::System.Buffers.ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                            while (!{{(isChar ? $"((ISpanFormattable)value.{name})" : $"value.{name}")}}.TryFormat(tmp, out charsWritten, default, config.CultureInfo))
                            {
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.EnsureBuffer(ref tmp);
                            }
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                            global::System.Buffers.ArrayPool<char>.Shared.Return(tmp);
                        }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NullableClassISpanFormattableSerializeCodegen(StringBuilder builder, string name)
    {
        builder.AppendFormatted($$"""

                        if (value.{{name}} is not null)
                        {
                            if (value.{{name}}.TryFormat(bufferSpan, out charsWritten, default, config.CultureInfo))
                            {
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
                            }
                            else
                            {
                                char[] tmp = global::System.Buffers.ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                                while (!value.{{name}}.TryFormat(tmp, out charsWritten, default, config.CultureInfo))
                                {
                                    global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.EnsureBuffer(ref tmp);
                                }
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                                global::System.Buffers.ArrayPool<char>.Shared.Return(tmp);
                            }
                        }
                        else
                        {
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, default, config, 0);
                        }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NullableStructISpanFormattableSerializeCodegen(StringBuilder builder, string name, bool isChar = false)
    {
        builder.AppendFormatted($$"""

                        if (value.{{name}} is not null)
                        {
                            if ({{(isChar ? $"((ISpanFormattable)value.{name}.Value)" : $"value.{name}.Value")}}.TryFormat(bufferSpan, out charsWritten, default, config.CultureInfo))
                            {
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
                            }
                            else
                            {
                                char[] tmp = global::System.Buffers.ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                                while (!{{(isChar ? $"((ISpanFormattable)value.{name}.Value)" : $"value.{name}.Value")}}.TryFormat(tmp, out charsWritten, default, config.CultureInfo))
                                {
                                    global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.EnsureBuffer(ref tmp);
                                }
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                                global::System.Buffers.ArrayPool<char>.Shared.Return(tmp);
                            }
                        }
                        else
                        {
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, default, config, 0);
                        }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NullableStringSerializeCodegen(StringBuilder builder, string name)
    {
        builder.AppendFormatted($$"""

                        if (value.{{name}} is not null)
                        {
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, value.{{name}}.AsSpan(), config, value.{{name}}.Length);
                        }
                        else
                        {
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, default, config, 0);
                        }

            """);
    }
    #endregion
    #endregion
}
