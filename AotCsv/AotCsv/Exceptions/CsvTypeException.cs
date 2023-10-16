using System.Diagnostics.CodeAnalysis;

namespace Oucc.AotCsv.Exceptions;

public class CsvTypeException : CsvBadDataException
{
    public Type TargetType { get; }

    public string FieldValue { get; }

    public string MemberName { get; }

    public override string Message => ToString();

    public CsvTypeException(Type targetType, string fieldValue, string memberName, Exception? innerException = null) : base(null, innerException)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        ArgumentNullException.ThrowIfNull(fieldValue);
        ArgumentNullException.ThrowIfNull(memberName);

        TargetType = targetType;
        FieldValue = fieldValue;
        MemberName = memberName;
    }

    [DoesNotReturn]
    public static void Throw(Type targetType, string fieldValue, string memberName)
        => throw new CsvTypeException(targetType, fieldValue, memberName);

    public override string ToString()
    {
        return $"Fail to parse type. Field Value:\"{FieldValue}\", Target Type:\"{TargetType}\", Member Name:\"{MemberName}\"";
    }
}
