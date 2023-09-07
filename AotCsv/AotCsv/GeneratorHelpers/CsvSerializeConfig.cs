using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oucc.AotCsv.GeneratorHelpers;
public record CsvSerializeConfig (QuoteOption QuoteOption,CultureInfo CultureInfo)
{
    public CsvSerializeConfig(CultureInfo cultureInfo) : this(QuoteOption.ShouldQuote, cultureInfo)
    {
    }
}
