using Microsoft.CodeAnalysis;

namespace Oucc.AotCsv.Generator.Utility;

internal static class SymbolFormat
{
    /// <summary>
    /// class Hoge&lt;T&gt; のような形で使用するとき用
    /// </summary>
    public static SymbolDisplayFormat NameOnly { get; } =
            new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                miscellaneousOptions:
                    SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                    SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
}
