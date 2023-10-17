namespace Oucc.AotCsv.Exceptions;

public class CsvBadDataException : AotCsvException
{
    public MappingMetadata Metadata { get; }

    public CsvBadDataException(MappingMetadata mappingMetadata) : base(null, null)
    {
        ArgumentNullException.ThrowIfNull(mappingMetadata);

        Metadata = mappingMetadata;
    }
}
