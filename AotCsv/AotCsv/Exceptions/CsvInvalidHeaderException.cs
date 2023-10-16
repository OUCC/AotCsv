using System.Diagnostics.CodeAnalysis;

namespace Oucc.AotCsv.Exceptions;

public class CsvInvalidHeaderException : CsvBadDataException
{
    public IReadOnlyList<int> ReadColumnMap;

    public CsvInvalidHeaderException(List<int> readColumnMap) : base(null, null)
    {
        ReadColumnMap = readColumnMap;
    }

    [DoesNotReturn]
    public static void Throw(List<int> readColumnMap) => throw new CsvInvalidHeaderException(readColumnMap);
}
