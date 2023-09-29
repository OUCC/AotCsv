using System.Globalization;
using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;

public class CsvReader
{
    private CsvParser _parser;

    public CsvDeserializeConfig Config { get; }

    public CsvReader(TextReader reader, CultureInfo cultureInfo) : this(reader,new CsvDeserializeConfig(cultureInfo)) { }

    public CsvReader(TextReader reader, CsvDeserializeConfig config) : this(new CsvParser(reader, config), config) { }

    private CsvReader(CsvParser parser, CsvDeserializeConfig config)
    {
        _parser = parser;
        Config = config;
    }

    public IEnumerable<T> GetRecords<T>() where T : ICsvSerializable<T>
    {
        //while (T.TryParse(_parser, out var result))
        //{
        //    yield return result;
        //}
        yield break;
    }
}
