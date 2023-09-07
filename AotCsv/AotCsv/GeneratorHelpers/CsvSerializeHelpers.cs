using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Oucc.AotCsv.GeneratorHelpers;

public class CsvSerializeHelpers
{
    /// <summary>
    /// 書き込む際にCSVとして正しくしてから書き込む
    /// </summary>
    // この属性はベンチマーク取ってつけるかつけないかを決める（おそらくメソッドが大きくてインライン化されない）
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteWithCheck(TextWriter writer, ReadOnlySpan<char> buffer, CsvSerializeConfig config, int writtenChars)
    {
        int nextIndex = buffer[..writtenChars].IndexOfAny("\r\n,\"");
        if (nextIndex == -1)
        {
            if (config.QuoteOption == QuoteOption.MustQuote)
            {
                writer.Write('"');
                writer.Write(buffer[..writtenChars]);
                writer.Write('"');
            }
            else
            {
                writer.Write(buffer[..writtenChars]);
            }
        }
        else if (config.QuoteOption == QuoteOption.ShouldQuote)
        {
            int writtenIndex = 0;
            int index = 0;
            writer.Write('"');
            while (true)
            {
                if (buffer[nextIndex] == '\r' || buffer[nextIndex] == '\n')
                {
                    nextIndex++;
                    if (nextIndex < buffer.Length && buffer[nextIndex] == '\n') nextIndex++;
                }
                else if (buffer[nextIndex] == ',')
                {
                    nextIndex++;
                }
                else
                {
                    writer.Write(buffer[writtenIndex..++nextIndex]);
                    writer.Write('"');
                    writtenIndex = nextIndex;
                }
                index = buffer[nextIndex..writtenChars].IndexOfAny("\r\n,\"");
                if (index == -1) break;
                nextIndex += index;
            }
            writer.Write(buffer[writtenIndex..writtenChars]);
            writer.Write('"');
        }
        else
        {
            AotCsvException.ThrowMustNoQuoteException();
        }
    }

    /// <summary>
    /// 与えられた配列を二倍の長さにする。拡大する際に元の値は保持されないので注意
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnsureBuffer(ref char[] buffer)
    {
        var tmp2 = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
        ArrayPool<char>.Shared.Return(buffer);
        buffer = tmp2;
    }
}
