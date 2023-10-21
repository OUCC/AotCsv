using System.Globalization;
using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;

public sealed class CsvReader : IDisposable
{
    private readonly CsvParser _parser;

    public CsvDeserializeConfig Config { get; }

    private bool _disposed;

    public CsvReader(TextReader reader, CultureInfo cultureInfo, bool leaveOpen = false) : this(reader, new CsvDeserializeConfig(cultureInfo) { LeaveOpen = leaveOpen }) { }

    public CsvReader(TextReader reader, CsvDeserializeConfig config) : this(new CsvParser(reader, config), config) { }

    private CsvReader(CsvParser parser, CsvDeserializeConfig config)
    {
        _parser = parser;
        Config = config;
    }

    public IEnumerable<T> GetRecords<T>() where T : ICsvSerializable<T>
    {
        if (_parser.ColumnMap == default)
        {
            T.ParseHeader(_parser, out var columnMap);
            _parser.ColumnMap = columnMap;
        }

        while (T.ParseRecord(_parser, out var result))
        {
            yield return result;
        }
        yield break;
    }

    public void Dispose()
    {
        if (!_disposed) return;

        _parser.Dispose();
        _disposed = true;
    }
}
