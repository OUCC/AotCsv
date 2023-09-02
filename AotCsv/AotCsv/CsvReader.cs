using System.Globalization;
using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;

public class CsvReader
{
    private CsvParser _parser;
    public CultureInfo Culture { get; }

    public CsvReader(TextReader reader, CultureInfo cultureInfo) : this(new CsvParser(reader, cultureInfo), cultureInfo) { }

    private CsvReader(CsvParser parser, CultureInfo culture)
    {
        _parser = parser;
        Culture = culture;
    }

    public IEnumerable<T> GetRecords<T>() where T : ICsvParsable<T>
    {
        while (T.TryParse(_parser, out var result))
        {
            yield return result;
        }
    }
}
