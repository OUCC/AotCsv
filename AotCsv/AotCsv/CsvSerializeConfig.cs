using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;

public record CsvSerializeConfig(QuoteOption QuoteOption, CultureInfo CultureInfo)
{
    public CsvSerializeConfig(CultureInfo cultureInfo) : this(QuoteOption.ShouldQuote, cultureInfo)
    {
    }
}
