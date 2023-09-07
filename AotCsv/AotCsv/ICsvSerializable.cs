using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;

public interface ICsvSerializable<T> where T : ICsvSerializable<T>
{
    //static abstract bool TryParse(CsvParser reader, out T value);

    static abstract void WriteRecord(TextWriter writer, CsvSerializeConfig context, T value);

    static abstract void WriteHeader(TextWriter writer, CsvSerializeConfig context);
}
