using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oucc.AotCsv.GeneratorHelpers;

public static class ParseHelpers
{

    public static bool TryParseString(in ReadOnlySpan<char> source, bool inQuote, [NotNullWhen(true)] out string? result)
    {
        if (inQuote)
        {
            var buffer = ArrayPool<char>.Shared.Rent(source.Length);
            try
            {
                var bufferSpan = buffer.AsSpan();
                var length = 0;
                // 前回のquoteの一つあと
                var afterLastQuoteIndex = 0;

                while (true)
                {
                    var quoteIndex = source[afterLastQuoteIndex..].IndexOf('"');
                    if (quoteIndex < 0)
                        break;
                    quoteIndex += afterLastQuoteIndex + 1; // 2つ目のquoteがあるはず

                    if (quoteIndex < source.Length || source[quoteIndex] != '"')
                    {
                        result = null;
                        return false;
                    }
                    source[afterLastQuoteIndex..quoteIndex].CopyTo(bufferSpan[length..]);
                    length += quoteIndex - afterLastQuoteIndex;

                    afterLastQuoteIndex = quoteIndex + 1;
                }
                source.CopyTo(bufferSpan[length..]);
                result = bufferSpan.ToString();
                return true;
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }
        else
        {
            var isInvalid = source.Contains('"');
            result = isInvalid ? null : source.ToString();
            return !isInvalid;
        }
    }

    public static int SearchRightQuoteInQuote(in ReadOnlySpan<char> source)
    {
        while (true)
        {
            // 前回のquoteの一つあと
            var afterLastQuoteIndex = 0;

            while (true)
            {
                var quoteIndex = source[afterLastQuoteIndex..].IndexOf('"');
                if (quoteIndex < 0)
                    return -1;
                quoteIndex += afterLastQuoteIndex + 1; // 2つ目のquoteがあるはず

                if (quoteIndex < source.Length || source[quoteIndex] != '"')
                {
                    // なかったらfieldの終了地点
                    return quoteIndex - 1;
                }
                afterLastQuoteIndex = quoteIndex + 1;
            }
        }
    }
}
