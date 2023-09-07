using System.Buffers;
using System.Runtime.CompilerServices;
using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv.ConsoleApp.GeneratedCodeTarget;

internal partial class SampleModel : ICsvSerializable<SampleModel>
{
    static bool ICsvSerializable<SampleModel>.TryParse(CsvParser reader, out SampleModel value)
    {
        throw new NotImplementedException();
    }

    static void ICsvSerializable<SampleModel>.WriteHeader(TextWriter writer, CsvSerializeConfig context)
    {

        if (context.QuoteOption == QuoteOption.MustQuote)
        {
            writer.WriteLine("\"ID\",\"名\",\"姓\",\"MiddleName\",\"BirthDay\"");
        }
        else
        {
            writer.WriteLine("ID,名,姓,MiddleName,BirthDay");
        }
    }

    static void ICsvSerializable<SampleModel>.WriteRecord(TextWriter writer, CsvSerializeConfig config, SampleModel value)
    {
        // この属性はベンチマーク取ってつけるかつけないかを決める（おそらくメソッドが大きくてインライン化されない）
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteWithCheck(TextWriter writer, ReadOnlySpan<char> buffer, CsvSerializeConfig config, int writtenChars)
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

        var buffer = ArrayPool<char>.Shared.Rent(HelperConst.BufferLength);
        var bufferSpan = buffer.AsSpan()[..HelperConst.BufferLength];
        var charsWritten = 0;
        try
        {
            // ISpanFormattableが実装されていないが、IFormattableが実装されている場合は
            // value.Id.ToString("G", config.CultureInfo).TryCopyTo(bufferSpan)
            // IFormattableすらない場合は
            // value.Id.ToString().TryCopyTo(bufferSpan)
            // .net standard 2.1 では string を AsSpan() する
            // "".AsSpan().TryCopyTo()

            // ID
            if (value.Id.TryFormat(bufferSpan, out charsWritten, default, config.CultureInfo))
            {
                WriteWithCheck(writer, bufferSpan, config, charsWritten);
            }
            else
            {
                // たいていの primitive では必要ないと思うけど、一応フォーマット文字列が1024文字を超える場合の処理
                var tmp = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                while (!value.Id.TryFormat(tmp, out charsWritten, provider: config.CultureInfo))
                {
                    var tmp2 = ArrayPool<char>.Shared.Rent(tmp.Length * 2);
                    tmp.AsSpan().CopyTo(tmp2);
                    ArrayPool<char>.Shared.Return(tmp);
                    tmp = tmp2;
                }
                WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                ArrayPool<char>.Shared.Return(tmp);
            }
            writer.Write(',');

            //FirstName
            WriteWithCheck(writer, value.FirstName.AsSpan(), config, value.FirstName.Length);
            writer.Write(',');

            //LastName
            WriteWithCheck(writer, value.LastName.AsSpan(), config, value.LastName.Length);
            writer.Write(',');

            //MiddleName
            if (value.MiddleName is not null)
            {
                WriteWithCheck(writer, value.MiddleName.AsSpan(), config, value.MiddleName.Length);
            }
            else
            {
                WriteWithCheck(writer, default, config, 0);
            }
            writer.Write(',');

            // BirthDay
            if (value.BirthDay.TryFormat(bufferSpan, out charsWritten, HelperConst.BirthDayFormat, config.CultureInfo))
            {
                WriteWithCheck(writer, bufferSpan, config, charsWritten);
            }
            else
            {
                var tmp = ArrayPool<char>.Shared.Rent(HelperConst.BufferLength * 2);
                while (!value.BirthDay.TryFormat(tmp, out charsWritten, HelperConst.BirthDayFormat, config.CultureInfo))
                {
                    var tmp2 = ArrayPool<char>.Shared.Rent(tmp.Length * 2);
                    tmp.AsSpan().CopyTo(tmp2);
                    ArrayPool<char>.Shared.Return(tmp);
                    tmp = tmp2;
                }
                WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                ArrayPool<char>.Shared.Return(tmp);
            }
            writer.WriteLine();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}

file class HelperConst
{
    // If your target CPU is ARM, this value should be 256 and if the CPU is ARM64 or LOONGARCH64, it should be nuint.MaxValue. 
    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Buffer.Unix.cs
    internal const int BufferLength = 1024;

    internal const string BirthDayFormat = "yyyy年MM月dd日";
}
