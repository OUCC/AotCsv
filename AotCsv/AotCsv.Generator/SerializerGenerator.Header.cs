using System.Text;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

public partial class SerializerGenerator
{
    private static ISymbol[] CreateHeaderCode(StringBuilder builder, INamedTypeSymbol targetSymbol, ReferenceSymbols reference)
    {
        var targetMembers = targetSymbol.GetMembers()
            .Where(m => m is IPropertySymbol p && IsTargetProperty(p, reference)
                || m is IFieldSymbol f && IsTargetField(f, reference))
            .ToArray();

        var header = targetMembers.Select(m =>
        {
            var nameAttribute = m.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToString() == "Oucc.AotCsv.Attributes.CsvNameAttribute");
            if (nameAttribute is null) return m.Name.Replace("\"", "\"\"");

            return nameAttribute.ConstructorArguments[0].Value as string ?? m.Name;
        }).ToArray();

        builder.AppendFormatted($$"""
                    if (context.QuoteOption == {{Constants.QuoteOption}}.MustQuote)
                    {
                        writer.WriteLine("
            """);

        for (var i = 0; i < header.Length; i++)
        {
            builder.AppendFormatted($@"\""{header[i]}\""");
            if (i < header.Length - 1)
                builder.Append(',');
        }

        builder.AppendFormatted($$"""
            ");
                    }
                    else
                    {
                        writer.WriteLine("
            """);


        for (var i = 0; i < header.Length; i++)
        {
            if (header[i].AsSpan().IndexOfAny('"', ',') >= 0) builder.AppendFormatted($@"\""{header[i]}\""");
            else builder.Append(header[i]);

            if (i < header.Length - 1)
                builder.Append(',');
        }

        builder.AppendFormatted($$"""
            ");
                    }

            """);

        return targetMembers;
    }

    private static bool IsTargetProperty(IPropertySymbol property, ReferenceSymbols reference)
    {
        return
            // 必須条件
            !property.IsStatic && !property.IsWriteOnly && !property.IsIndexer && IsTargetType(property.Type, reference)
            && property.GetAttributes().All(a => !reference.CsvIgnoreAttribute.Equals(a.AttributeClass, SymbolEqualityComparer.Default))
            // デフォルトの条件
            && (property.DeclaredAccessibility == Accessibility.Public && !property.IsReadOnly
                // 属性がついていたとき
                || property.GetAttributes().Any(a => reference.CsvIncludeAttribute.Equals(a.AttributeClass, SymbolEqualityComparer.Default)));
    }


    private static bool IsTargetField(IFieldSymbol field, ReferenceSymbols reference)
    {
        return
            // 必須条件
            !field.IsStatic && IsTargetType(field.Type, reference)
            && field.GetAttributes().All(a => !reference.CsvIgnoreAttribute.Equals(a.AttributeClass, SymbolEqualityComparer.Default))
            // デフォルトの条件
            && (field.DeclaredAccessibility == Accessibility.Public && !field.IsReadOnly
                // 属性がついていたとき
                || field.GetAttributes().Any(a => reference.CsvIncludeAttribute.Equals(a.AttributeClass, SymbolEqualityComparer.Default)));
    }

    private static bool IsTargetType(ITypeSymbol type, ReferenceSymbols reference)
    {
        return type.AllInterfaces.Contains(reference.ISpanFormattable) || type.Equals(reference.String, SymbolEqualityComparer.Default);
    }
}
