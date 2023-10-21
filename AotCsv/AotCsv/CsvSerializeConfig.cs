using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;

public record CsvSerializeConfig
{
    public QuoteOption QuoteOption { get; init; }
    public NewLineOption NewLineOption { get; init; }
    public CultureInfo CultureInfo { get; init; }
    public string NewLine { get; init; }
    public CsvSerializeConfig(CultureInfo cultureInfo) : this(QuoteOption.ShouldQuote, NewLineOption.Environment, cultureInfo) { }
    public CsvSerializeConfig(QuoteOption quoteOption, CultureInfo cultureInfo) : this(quoteOption, NewLineOption.Environment, cultureInfo) { }

    public CsvSerializeConfig(QuoteOption quoteOption, NewLineOption newLineOption, CultureInfo cultureInfo)
    {
        QuoteOption = quoteOption;
        NewLineOption = newLineOption;
        CultureInfo = cultureInfo;
        NewLine = newLineOption switch
        {
            NewLineOption.Environment => Environment.NewLine,
            NewLineOption.CR => "\r",
            NewLineOption.LF => "\n",
            NewLineOption.CRLF => "\r\n",
            _ => "\n"
        };
    }
}
