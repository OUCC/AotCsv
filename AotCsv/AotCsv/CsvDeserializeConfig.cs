using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oucc.AotCsv;

public record CsvDeserializeConfig(ReadQuote ReadQuote, CultureInfo CultureInfo)
{
    public CsvDeserializeConfig(CultureInfo cultureInfo) : this(ReadQuote.Auto, cultureInfo) { }
}
