using System.Diagnostics.CodeAnalysis;

namespace Oucc.AotCsv.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class CsvDateTimeFormatAttribute : Attribute
{
    public string DateFormat { get; }

    public CsvDateTimeFormatAttribute([StringSyntax(StringSyntaxAttribute.DateTimeFormat)] string dateFormat)
    {
        DateFormat = dateFormat;
    }
}
