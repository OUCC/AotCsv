﻿using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Oucc.AotCsv.Generator.Comparer;
using Oucc.AotCsv.Generator.Utility;

namespace Oucc.AotCsv.Generator;

[Generator(LanguageNames.CSharp)]
public class SerializerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var attributeContext = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Oucc.AotCsv.Attributes.CsvSerializableAttribute",
            static (_, _) => true,
            static (context, _) => context);

        var source = attributeContext.Combine(context.CompilationProvider).WithComparer(GeneratorComparer.Instance);

        context.RegisterSourceOutput(source, static (context, source) =>
        {
            var (attributeContext, compilation) = source;

            Emit(context, attributeContext, compilation);
        });
    }

    private static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source, Compilation compilation)
    {
        var cancellationToken = context.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        var targetSymbol = (INamedTypeSymbol)source.TargetSymbol;
        var targetTypeName = targetSymbol.ToDisplayString(SymbolFormat.NameOnly);

        var builder = new StringBuilder();
        var reference = new ReferenceSymbols(compilation);
        var targets = GetTargetMembers(targetSymbol, reference, cancellationToken);
        var containingTypes = GetTargetSymbolContainingTypes(targetSymbol, cancellationToken);
        var helperClassName = targetSymbol.Arity == 0
            ? "Helper"
            : string.Concat("Helper<", string.Join(", ", targetSymbol.TypeParameters.Select(t => t.ToDisplayString(SymbolFormat.NameOnly))), ">");
        var fullHelperClassName = string.Concat(targetSymbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), ".", helperClassName);

        cancellationToken.ThrowIfCancellationRequested();
        builder.Append("""
            // <auto-generated/>
            #nullable enable

            """);

        if (!targetSymbol.ContainingNamespace.IsGlobalNamespace)
            builder.AppendFormatted($"namespace {targetSymbol.ContainingNamespace.ToString()};\n");

        foreach (var containingType in containingTypes)
        {
            builder.AppendFormatted($$"""

                partial {{(containingType.IsRecord ? "record " : "")}}{{(containingType.TypeKind == TypeKind.Class ? "class" : "struct")}} {{containingType.ToDisplayString(SymbolFormat.NameOnly)}} {
                """);
        }

        builder.AppendFormatted($$"""

            partial {{(targetSymbol.IsRecord ? "record " : "")}}{{(targetSymbol.TypeKind == TypeKind.Class ? "class" : "struct")}} {{targetTypeName}} : global::Oucc.AotCsv.ICsvSerializable<{{targetTypeName}}>
            {
                static void global::Oucc.AotCsv.ICsvSerializable<{{targetTypeName}}>.WriteHeader(global::System.IO.TextWriter writer, global::Oucc.AotCsv.CsvSerializeConfig context)
                {

            """);

        SerializeCodeGenerator.CreateHeaderCode(targets, builder, cancellationToken);

        builder.AppendFormatted($$"""
                }

                static void global::Oucc.AotCsv.ICsvSerializable<{{targetTypeName}}>.WriteRecord(global::System.IO.TextWriter writer, global::Oucc.AotCsv.CsvSerializeConfig config, {{targetTypeName}} value)
                {
            """);

        SerializeCodeGenerator.CreateBodyCode(builder, targets, reference, cancellationToken);

        builder.Append("""
                }

           """);

        DeserializeCodeGenerator.WriteHeaderCode(builder, targets, targetTypeName, fullHelperClassName, cancellationToken);

        DeserializeCodeGenerator.WriteBodyCode(builder, targets, targetSymbol, reference, fullHelperClassName, cancellationToken);

        for (int i = 0; i < containingTypes.Count + 1; i++)
        {
            builder.Append("}\n");
        }

        WriteMetadata(builder, helperClassName, targetTypeName, targetSymbol, targets, cancellationToken);

        builder.Append("#nullable restore\n");
        var result = builder.ToString();

        context.AddSource(targetSymbol.Name + ".g.cs", result);
    }

    private static void WriteMetadata(StringBuilder builder, string className, string targetTypeName, INamedTypeSymbol targetType, MemberMeta[] members, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        builder.AppendFormatted($$"""
            file static class {{className}}
            """);
        if (targetType.Arity > 0)
        {
            foreach (var typePrameter in targetType.TypeParameters)
            {
                if (typePrameter.ConstraintTypes.Length == 0
                    && !typePrameter.HasNotNullConstraint
                    && !typePrameter.HasReferenceTypeConstraint
                    && !typePrameter.HasValueTypeConstraint
                    && !typePrameter.HasUnmanagedTypeConstraint)
                    continue;
                builder.AppendFormatted($$"""

                            where {{typePrameter.Name}} :
                    """);
                var appendedFirst = false;
                if (typePrameter.HasNotNullConstraint)
                {
                    builder.Append(" notnull");
                    appendedFirst = true;
                }
                else if (typePrameter.HasReferenceTypeConstraint)
                {
                    builder.Append(" class");
                    appendedFirst = true;
                }
                else if (typePrameter.HasValueTypeConstraint)
                {
                    builder.Append(" struct");
                    appendedFirst = true;
                }
                else if (typePrameter.HasUnmanagedTypeConstraint)
                {
                    builder.Append(" unmanaged");
                    appendedFirst = true;
                }

                foreach (var constraintType in typePrameter.ConstraintTypes)
                {
                    if (appendedFirst) builder.Append(", ");
                    else builder.Append(' ');
                    builder.Append(constraintType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }

                if (typePrameter.HasConstructorConstraint)
                {
                    if (appendedFirst) builder.Append(", new()");
                    else builder.Append(" new()");
                }
            }
        }
        builder.AppendFormatted($$"""

            {
                public static global::Oucc.AotCsv.MappingMetadata MappingMetadata => new(
                        typeof({{targetTypeName}}),
                        global::System.Collections.Immutable.ImmutableArray.Create<global::Oucc.AotCsv.MemberMetadata>(

            """);


        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];

            builder.AppendFormatted($$"""
                                new global::Oucc.AotCsv.MemberMetadata({{member.InternalId}}, typeof({{member.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}}), @"{{member.Symbol.Name}}", @"{{member.HeaderName}}", {{member.Index}}
                """);
            WriteString(builder, member.Format);
            WriteString(builder, member.DateTimeFormat.Item1);
            builder.AppendFormatted($$"""
                , (global::System.Globalization.DateTimeStyles){{(int)member.DateTimeFormat.Item2}}
                """);
            builder.AppendFormatted($$"""
                , {{(member.State & MemberMeta.MemberState.Property) == MemberMeta.MemberState.Property}}
                """);
            builder.AppendFormatted($$"""
                , {{(member.State & MemberMeta.MemberState.Settable) == MemberMeta.MemberState.Settable}}
                """);
            builder.AppendFormatted($$"""
                , {{(member.State & MemberMeta.MemberState.Readable) == MemberMeta.MemberState.Readable}})
                """);

            if (i < members.Length - 1)
            {
                builder.Append(",\n");
            }
        }


        builder.Append("""
                        )
                    );
            }

            """);

        static void WriteString(StringBuilder builder, string? value)
        {

            if (value is null) builder.Append(", null");
            else
            {
                builder.Append(", @\"");
                builder.Append(value);
                builder.Append('"');
            }
        }
    }

    private static List<ITypeSymbol> GetTargetSymbolContainingTypes(INamedTypeSymbol targetSymbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = new List<ITypeSymbol>();
        var containingType = targetSymbol.ContainingType;
        while (true)
        {
            if (containingType is null)
            {
                break;
            }
            else
            {
                result.Add(containingType);
            }
            containingType = containingType.ContainingType;
        }
        result.Reverse();
        return result;
    }

    private static MemberMeta[] GetTargetMembers(INamedTypeSymbol targetSymbol, ReferenceSymbols reference, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = targetSymbol.GetMembers()
            .Select(MemberMeta (m, i) =>
            {
                // Whereで null reference type を絞れないため!をつける
                if (m is IPropertySymbol p)
                    return IsTargetProperty(p, reference) ? new MemberMeta(i + 1, p) : null!;
                if (m is IFieldSymbol f)
                    return IsTargetField(f, reference) ? new MemberMeta(i + 1, f) : null!;
                return null!;
            })
            .Where(m => m is not null)
            .OrderBy(m => m.Index ?? uint.MaxValue)
            .ToArray();
        return result;
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
        if (type.NullableAnnotation == NullableAnnotation.Annotated && type.IsValueType)
        {
            var typeArgument = (type as INamedTypeSymbol)!.TypeArguments[0];
            if (typeArgument is ITypeParameterSymbol typeParameter)
            {
                return IsValidConstraint(typeParameter, typeParameter.ConstraintTypes, reference);
            }
            return IsValidConstraint(typeArgument, typeArgument.AllInterfaces, reference)
                || typeArgument.Equals(reference.Boolean, SymbolEqualityComparer.Default);// boolのAnnotatedはここで拾う
        }
        else if (type is ITypeParameterSymbol typeParameter)
        {
            return IsValidConstraint(type, typeParameter.ConstraintTypes, reference);
        }
        else
        {
            return IsValidConstraint(type, type.AllInterfaces, reference)
                || type.Equals(reference.String, SymbolEqualityComparer.Default)
                || type.Equals(reference.Boolean, SymbolEqualityComparer.Default);// boolのNotAnnotatedはここで拾える
        }

        static bool IsValidConstraint<T>(ITypeSymbol baseType, ImmutableArray<T> types, ReferenceSymbols reference) where T : ITypeSymbol
        {
            var count = 0;
            foreach (var t in types)
            {
                if (t is INamedTypeSymbol namedType)
                {
                    if (namedType.Equals(reference.ISpanFormattable, SymbolEqualityComparer.Default))
                        count++;
                    else if (namedType.OriginalDefinition.Equals(reference.ISpanParsable_T, SymbolEqualityComparer.Default)
                            && namedType.TypeArguments.Length == 1
                            && namedType.TypeArguments[0].Equals(baseType, SymbolEqualityComparer.Default))
                        count++;
                }

                if (count >= 2) return true;
            }

            return false;
        }
    }
}
