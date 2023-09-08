using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

public partial class SerializerGenerator
{
    private static void CreateBodyCode(StringBuilder builder, INamedTypeSymbol targetSymbol, ISymbol[] targetMemberSymbols, ReferenceSymbols reference)
    {
        if (targetMemberSymbols.Length == 0)
        {
            targetMemberSymbols = targetSymbol.GetMembers().Where(x => !x.IsImplicitlyDeclared && x is IPropertySymbol or IFieldSymbol).ToArray();
        }

        builder.Append("""

                    char[] buffer = global::System.Buffers.ArrayPool<char>.Shared.Rent(1024);
                    Span<char> bufferSpan = buffer.AsSpan()[..1024];
                    int charsWritten = 0;
                    try
                    {
            """);

        for (var i = 0; i < targetMemberSymbols.Length; i++)
        {
            var symbol = targetMemberSymbols[i];
            ITypeSymbol typeSymbol;
            if (symbol is IPropertySymbol propertySymbol)
            {
                typeSymbol = propertySymbol.Type;
            }
            else if (symbol is IFieldSymbol fieldSymbol)
            {
                typeSymbol = fieldSymbol.Type;
            }
            else continue;

            if (typeSymbol.Equals(reference.String, SymbolEqualityComparer.Default))
            {
                if (typeSymbol.NullableAnnotation == NullableAnnotation.NotAnnotated)
                {
                    StringSerializeCodegen(builder, symbol.Name);
                }
                else
                {
                    NullableStringSerializeCodegen(builder, symbol.Name);
                }
            }
            else if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated && typeSymbol.TypeKind == TypeKind.Struct)
            {
                var typeParameter = (typeSymbol as INamedTypeSymbol)!.TypeArguments[0];
                if (typeParameter.Equals(reference.DateTime, SymbolEqualityComparer.IncludeNullability))
                {
                    DateTimeSerializeCodegen(builder, symbol, reference, true, typeParameter.Name);
                }
                else if (typeParameter.AllInterfaces.Contains(reference.ISpanFormattable))
                {
                    NullableIStructSpanFormattableSerializeCodegen(builder, symbol.Name, typeParameter.Name);
                }
            }
            else if (typeSymbol.NullableAnnotation != NullableAnnotation.NotAnnotated && typeSymbol.TypeKind == TypeKind.Class)
            {
                NullableIClassSpanFormattableSerializeCodegen(builder, symbol.Name);
            }
            else
            {
                if (typeSymbol.Equals(reference.DateTime, SymbolEqualityComparer.IncludeNullability))
                {
                    DateTimeSerializeCodegen(builder, symbol, reference, false, "");
                }
                else if (typeSymbol.AllInterfaces.Contains(reference.ISpanFormattable))
                {
                    ISpanFormattableSerializeCodegen(builder, symbol.Name);
                }
            }

            if (i < targetMemberSymbols.Length - 1)
            {
                builder.Append("""
                            writer.Write(',');

                """);
            }
        }

        builder.Append("""

                        writer.WriteLine();
                    }
                    finally
                    {
                        global::System.Buffers.ArrayPool<char>.Shared.Return(buffer);
                    }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DateTimeSerializeCodegen(StringBuilder builder, ISymbol propertySymbol, ReferenceSymbols reference, bool nullable, string type)
    {
        var attributes = propertySymbol.GetAttributes();
        if (attributes.IsEmpty)
        {
            if (nullable)
            {
                NullableIStructSpanFormattableSerializeCodegen(builder, propertySymbol.Name, type);
            }
            else ISpanFormattableSerializeCodegen(builder, propertySymbol.Name);
        }
        else
        {
            var formatAttribute = attributes.Where(x => x.AttributeClass!.Equals(reference.CsvDateTimeFormatAttribute, SymbolEqualityComparer.IncludeNullability)).First();
            if (formatAttribute != null)
            {
                string format = (string?)formatAttribute.ConstructorArguments[0].Value ?? "";
                if (nullable)
                {
                    NullableDateTimeSerializeCodegenWithFormat(builder, propertySymbol.Name, format, type);
                }
                else
                {
                    DateTimeSerializeCodegenWithFormat(builder, propertySymbol.Name, format);
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
    private static void NullableDateTimeSerializeCodegenWithFormat(StringBuilder builder, string name, string format, string type)
    {
        builder.AppendFormatted($$"""

                        if (value.{{name}} is not null)
                        {
                            {{type}} nullableValue = value.{{name}}.Value;
                            if (nullableValue.TryFormat(bufferSpan, out charsWritten, "{{format}}", config.CultureInfo))
                            {
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
                            }
                            else
                            {
                                char[] tmp = global::System.Buffers.ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                                while (!nullableValue.TryFormat(tmp, out charsWritten, "{{format}}", config.CultureInfo))
                                {
                                    global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.EnsureBuffer(ref tmp);
                                }
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                                global::System.Buffers.ArrayPool<char>.Shared.Return(tmp);
                            }
                        }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ISpanFormattableSerializeCodegen(StringBuilder builder, string name)
    {
        builder.AppendFormatted($$"""

                        if (value.{{name}}.TryFormat(bufferSpan, out charsWritten, default, config.CultureInfo))
                        {
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
                        }
                        else
                        {
                            char[] tmp = global::System.Buffers.ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                            while (!value.{{name}}.TryFormat(tmp, out charsWritten, provider: config.CultureInfo))
                            {
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.EnsureBuffer(ref tmp);
                            }
                            global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                            global::System.Buffers.ArrayPool<char>.Shared.Return(tmp);
                        }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NullableIClassSpanFormattableSerializeCodegen(StringBuilder builder, string name)
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
    private static void NullableIStructSpanFormattableSerializeCodegen(StringBuilder builder, string name, string type)
    {
        builder.AppendFormatted($$"""

                        if (value.{{name}} is not null)
                        {
                            {{type}} nullableValue = value.{{name}}.Value;
                            if (nullableValue.TryFormat(bufferSpan, out charsWritten, default, config.CultureInfo))
                            {
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
                            }
                            else
                            {
                                char[] tmp = global::System.Buffers.ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                                while (!nullableValue.TryFormat(tmp, out charsWritten, default, config.CultureInfo))
                                {
                                    global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.EnsureBuffer(ref tmp);
                                }
                                global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                                global::System.Buffers.ArrayPool<char>.Shared.Return(tmp);
                            }
                        }

            """);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void StringSerializeCodegen(StringBuilder builder, string name)
    {
        builder.AppendFormatted($"""

                        global::Oucc.AotCsv.GeneratorHelpers.CsvSerializeHelpers.WriteWithCheck(writer, value.{name}.AsSpan(), config, value.{name}.Length);

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
}
