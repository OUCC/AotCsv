using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;

public interface ICsvSerializable<T> where T : ICsvSerializable<T>
{
    static abstract bool TryParse(CsvParser reader, out T value);

    static abstract bool TryWrite(CsvWriter reader, T value);
}
