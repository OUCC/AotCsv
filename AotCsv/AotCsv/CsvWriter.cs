namespace Oucc.AotCsv;

public class CsvWriter<T> : IDisposable where T : ICsvSerializable<T>
{
    public CsvSerializeConfig Context { get; }
    private bool _disposed = false;
    private TextWriter _writer;

    public CsvWriter(TextWriter writer, CsvSerializeConfig context)
    {
        _writer = writer;
        Context = context;
    }

    public void WriteRecords(IEnumerable<T> records)
    {
        foreach (var record in records)
        {
            T.WriteRecord(_writer, Context, record);
        }
    }

    public void WriteHeader() 
    {
        T.WriteHeader(_writer, Context);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if(!_disposed)
        {
            if (disposing)
            {
                _writer.Dispose();
                _writer = TextWriter.Null;
            }
            _disposed = true;
        }
    }
}
