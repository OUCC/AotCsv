using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;

public interface ICsvParsable<T> where T : ICsvParsable<T>
{
    static abstract bool TryParse(CsvParser reader, out T value);

    static abstract bool TryWrite(CsvWriter reader, T value);
}
