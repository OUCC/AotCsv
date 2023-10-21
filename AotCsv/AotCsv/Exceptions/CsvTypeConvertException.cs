using System.Diagnostics.CodeAnalysis;

namespace Oucc.AotCsv.Exceptions;

public class CsvTypeConvertException : CsvBadDataException
{
    public string FieldValue { get; }
    public MemberMetadata Member { get; }

    public override string Message => ToString();

    public CsvTypeConvertException(string fieldValue, int internalId, MappingMetadata mappingMetadata) : base(mappingMetadata)
    {
        FieldValue = fieldValue;
        Member = mappingMetadata.Members.Single(m => m.InternalId == internalId);
    }

    [DoesNotReturn]
    public static void Throw(string fieldValue, int internalId, MappingMetadata mappingMetadata)
        => throw new CsvTypeConvertException(fieldValue, internalId, mappingMetadata);

    public override string ToString()
    {
        return $"Fail to parse type. Field Value:\"{FieldValue}\", Target Type:\"{Member.Type}\", Member Name:\"{Member.Name}\"";
    }
}
