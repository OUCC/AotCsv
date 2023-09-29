using System.Diagnostics.CodeAnalysis;
using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;

public interface ICsvSerializable<T> where T : ICsvSerializable<T>
{
    static abstract void WriteRecord(TextWriter writer, CsvSerializeConfig context, T value);
    static abstract bool TryParseHeader(CsvParser parser, [NotNullWhen(true)] out int[]? columnMap);

    static abstract bool TryParse(CsvParser reader, [NotNullWhen(true)] out T? value);

    static abstract void WriteHeader(TextWriter writer, CsvSerializeConfig context);
}
