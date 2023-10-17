using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oucc.AotCsv.Exceptions;

public class TooFewColumnsException : CsvBadDataException
{
    public int ReadColumnsCount { get; }

    public int RequiredColumnsCount { get; }

    public override string Message => ToString();

    private TooFewColumnsException(MappingMetadata mappingMetadata, int readColumnsCount, int requiredColumnsCount) : base(mappingMetadata) {
        RequiredColumnsCount = requiredColumnsCount;
        ReadColumnsCount = readColumnsCount;
    }

    public override string ToString()
    {
        return $"{RequiredColumnsCount} columns required, but only {ReadColumnsCount}";
    }

    public static void Throw(MappingMetadata mappingMetadata, int readColumnsCount, int requiredColumnsCount)
        => throw new TooFewColumnsException(mappingMetadata, readColumnsCount, requiredColumnsCount);
}
