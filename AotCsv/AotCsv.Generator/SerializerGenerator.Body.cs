using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

public partial class SerializerGenerator
{
    private static void CreateBodyCode(StringBuilder builder, INamedTypeSymbol targetSymbol, ISymbol[] targetMemberSymbols, ReferenceSymbols reference)
    {
#if DEBUG
        if (targetMemberSymbols.Length == 0)
        {
            targetMemberSymbols = targetSymbol.GetMembers().Where(x => !x.IsImplicitlyDeclared && x is IPropertySymbol or IFieldSymbol).ToArray();
        }
#endif

        builder.Append("""

                    char[] buffer = global::System.Buffers.ArrayPool<char>.Shared.Rent(1024);
                    global::System.Span<char> bufferSpan = buffer.AsSpan()[..1024];
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
                NullableStringSerializeCodegen(builder, symbol.Name);
            }
            else if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated && typeSymbol.TypeKind == TypeKind.Struct)
            {
                var typeParameter = (typeSymbol as INamedTypeSymbol)!.TypeArguments[0];
                if (typeParameter.Equals(reference.DateTime, SymbolEqualityComparer.IncludeNullability))
                {
                    DateTimeSerializeCodegen(builder, symbol, reference, true, typeParameter.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.FullyQualifiedFormat));
                }
                else if(typeParameter.Equals(reference.Boolean, SymbolEqualityComparer.IncludeNullability))
                {
                    NullableBooleanSerializeCodegen(builder, symbol.Name);
                }
                else if (typeParameter.AllInterfaces.Contains(reference.ISpanFormattable))
                {
                    NullableStructISpanFormattableSerializeCodegen(builder, symbol.Name, typeParameter.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.FullyQualifiedFormat));
                }
            }
            else if (typeSymbol.NullableAnnotation != NullableAnnotation.NotAnnotated && typeSymbol.TypeKind == TypeKind.Class)
            {
                var typeParameter = (typeSymbol as INamedTypeSymbol)!.TypeArguments[0];
                if (typeParameter.AllInterfaces.Contains(reference.ISpanFormattable))
                {
                    NullableClassISpanFormattableSerializeCodegen(builder, symbol.Name);
                }
            }
            else
            {
                if (typeSymbol.Equals(reference.DateTime, SymbolEqualityComparer.IncludeNullability))
                {
                    DateTimeSerializeCodegen(builder, symbol, reference, false, "");
                }
                else if (typeSymbol.Equals(reference.Boolean, SymbolEqualityComparer.IncludeNullability))
                {
                    BooleanSerializeCodegen(builder, symbol.Name);
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
    private static void DateTimeSerializeCodegen(StringBuilder builder, ISymbol propertySymbol, ReferenceSymbols reference, bool nullable)
    {
        var attributes = propertySymbol.GetAttributes();
        if (attributes.IsEmpty)
        {
            if (nullable)
            {
                NullableStructISpanFormattableSerializeCodegen(builder, propertySymbol.Name, type);
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
                            while (!value.{{name}}.TryFormat(tmp, out charsWritten, default, config.CultureInfo))
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
    private static void NullableStructISpanFormattableSerializeCodegen(StringBuilder builder, string name, string type)
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
