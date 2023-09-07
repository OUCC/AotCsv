using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv;


[Serializable]
public class AotCsvException : Exception
{
    public AotCsvException() { }
    public AotCsvException(string message) : base(message) { }
    public AotCsvException(string message, Exception inner) : base(message, inner) { }
    protected AotCsvException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowMustNoQuoteException()
    {
        throw new AotCsvException("QuoteOption have to be MustQuote or ShouldQuote");
    }
}
