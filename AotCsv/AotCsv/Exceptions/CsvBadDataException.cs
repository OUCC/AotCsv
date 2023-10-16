using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oucc.AotCsv.Exceptions;

public class CsvBadDataException : AotCsvException
{
    private CsvBadDataException() { }
    private CsvBadDataException(string? message) : base(message) { }
    public CsvBadDataException(string? message, Exception? inner) : base(message, inner) { }
    protected CsvBadDataException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
