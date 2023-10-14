using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Oucc.AotCsv;


[Serializable]
public class AotCsvException : Exception
{
    public AotCsvException() { }
    public AotCsvException(string? message) : base(message) { }
    public AotCsvException(string? message, Exception? inner) : base(message, inner) { }
    protected AotCsvException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowMustNoQuoteException()
    {
        throw new AotCsvException("QuoteOption have to be MustQuote or ShouldQuote");
    }

    [DoesNotReturn]
    public static void ThrowBaseException(string? message = null, Exception? innerException = null) => throw new AotCsvException(message, innerException);
}
