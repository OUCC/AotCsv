using System.Globalization;

namespace Oucc.AotCsv;

public record CsvDeserializeConfig(CultureInfo CultureInfo, ReadQuote ReadQuote, bool HasHeader, bool LeaveOpen)
{
    public static CsvDeserializeConfig InvariantCulture { get; } = new CsvDeserializeConfig(CultureInfo.InvariantCulture);

    public CsvDeserializeConfig(CultureInfo cultureInfo) : this(cultureInfo, ReadQuote.Auto, false, false) { }
}
