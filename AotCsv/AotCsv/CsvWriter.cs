using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;

public class CsvWriter
{
    public CsvSerializeConfig Context { get; }
    private TextWriter _writer;

    public CsvWriter(TextWriter writer, CsvSerializeConfig context)
    {
        _writer = writer;
        Context = context;
    }

    public void WriteRecords<T>(IEnumerable<T> records) where T : ICsvSerializable<T>
    {
        foreach (var record in records)
        {
            T.WriteRecord(_writer, Context, record);
        }
    }

    public void WriteHeader<T>() where T : ICsvSerializable<T>
    {
        T.WriteHeader(_writer, Context);
    }
}
