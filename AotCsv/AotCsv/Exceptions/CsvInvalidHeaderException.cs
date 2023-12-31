﻿using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Oucc.AotCsv.Exceptions;

public class CsvInvalidHeaderException : CsvBadDataException
{
    public IReadOnlyList<int> ReadColumnMap;

    public override string Message => ToString();

    private CsvInvalidHeaderException(List<int> readColumnMap, MappingMetadata mappingMetadata) : base(mappingMetadata)
    {
        ReadColumnMap = readColumnMap;
    }

    public override string ToString()
    {
        var columnMap = Metadata.Members.Where(m => m.IsWritable)
            .Select((meta, i) => (meta, ReadColumnMap.Contains(i + 1)))
            .Where(p => !p.Item2);

        var builder = new StringBuilder();
        builder.Append("Header: ");
        foreach(var (meta, _) in columnMap)
        {
            builder.Append($"\"{meta.HeaderName}\" ");
        }
        builder.Append("does not found.");
        return builder.ToString();
    }

    [DoesNotReturn]
    public static void Throw(List<int> readColumnMap, MappingMetadata mappingMetadata) => throw new CsvInvalidHeaderException(readColumnMap, mappingMetadata);
}
