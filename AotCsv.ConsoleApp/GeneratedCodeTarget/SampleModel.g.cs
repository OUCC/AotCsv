#nullable enable
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Oucc.AotCsv.Exceptions;
using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv.ConsoleApp.GeneratedCodeTarget;

internal partial class SampleModel : ICsvSerializable<SampleModel>
{

    public static void ParseHeader(CsvParser parser, out ImmutableArray<int> columnMap)
    {
        if (!parser.Config.HasHeader)
        {
            columnMap = ImmutableArray.Create<int>(1, 2, 3, 4, 5);
            return;
        }

        var rawColumnMap = new List<int>(5);
        var readValidCoulmns = 0;

        while (readValidCoulmns < 5)
        {
            using var field = parser.TryGetField(out var state);
            if (state == FieldState.NoLine)
            {
                AotCsvException.ThrowBaseException();
                columnMap = default;
                return;
            }

            var value = field.AsSpan() switch
            {
                "ID" => 1,
                "名" => 2,
                "姓" => 3,
                "MiddleName" => 4,
                "BirthDay" => 5,
                _ => 0
            };
            rawColumnMap.Add(value);
            if (value > 0)
                readValidCoulmns++;

            if (state == FieldState.LastField)
            {
                break;
            }
        }

        if (readValidCoulmns < 5)
        {
            CsvInvalidHeaderException.Throw(rawColumnMap);
            columnMap = default;
            return;
        }

        columnMap = ImmutableArray.Create(CollectionsMarshal.AsSpan(rawColumnMap));
    }

    static bool ICsvSerializable<SampleModel>.ParseRecord(CsvParser parser, [NotNullWhen(true)] out SampleModel? value)
    {
        int @Id = default;
        string @FirstName = "";
        string? @MiddleName = default;
        string @LastName = "";
        DateTime @BirthDay = default;


        int targetIndex;

        for (int columnIndex = 0; columnIndex < parser.ColumnMap.Length; columnIndex++)
        {
            targetIndex = parser.ColumnMap[columnIndex];
            using var t = parser.TryGetField(out var state);
            var field = t.AsSpan();

            if (state == FieldState.NoLine)
            {
                value = null;
                return false;
            }

            switch (targetIndex)
            {
                case 1:
                    if (!int.TryParse(field, parser.Config.CultureInfo, out @Id))
                    {
                        CsvTypeException.Throw(typeof(int), field.ToString(), @"Id");
                        value = null;
                        return false;
                    }
                    break;
                case 2:
                    @FirstName = field.ToString();
                    break;
                case 3:
                    @MiddleName = field.ToString();
                    break;
                case 4:
                    @LastName = field.ToString();
                    break;
                case 5:
                    if (!DateTime.TryParseExact(field, "yyyy年MM月dd日", parser.Config.CultureInfo, DateTimeStyles.None, out @BirthDay))
                    {
                        CsvTypeException.Throw(typeof(DateTime), field.ToString(), @"BirthDay");
                        value = null;
                        return false;
                    }
                    break;
            }
        }

        value = new @SampleModel()
        {
            Id = @Id!,
            FirstName = @FirstName!,
            MiddleName = @MiddleName!,
            LastName = @LastName!,
            BirthDay = @BirthDay!,
        };
        return true;
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
                CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
            }
            else
            {
                // たいていの primitive では必要ないと思うけど、一応フォーマット文字列が1024文字を超える場合の処理
                var tmp = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                while (!value.Id.TryFormat(tmp, out charsWritten, provider: config.CultureInfo))
                {
                    CsvSerializeHelpers.EnsureBuffer(ref tmp);
                }
                CsvSerializeHelpers.WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
                ArrayPool<char>.Shared.Return(tmp);
            }
            writer.Write(',');

            //FirstName
            CsvSerializeHelpers.WriteWithCheck(writer, value.FirstName.AsSpan(), config, value.FirstName.Length);
            writer.Write(',');

            //LastName
            CsvSerializeHelpers.WriteWithCheck(writer, value.LastName.AsSpan(), config, value.LastName.Length);
            writer.Write(',');

            //MiddleName
            if (value.MiddleName is not null)
            {
                CsvSerializeHelpers.WriteWithCheck(writer, value.MiddleName.AsSpan(), config, value.MiddleName.Length);
            }
            else
            {
                CsvSerializeHelpers.WriteWithCheck(writer, default, config, 0);
            }
            writer.Write(',');

            // BirthDay
            if (value.BirthDay.TryFormat(bufferSpan, out charsWritten, HelperConst.BirthDayFormat, config.CultureInfo))
            {
                CsvSerializeHelpers.WriteWithCheck(writer, bufferSpan, config, charsWritten);
            }
            else
            {
                var tmp = ArrayPool<char>.Shared.Rent(HelperConst.BufferLength * 2);
                while (!value.BirthDay.TryFormat(tmp, out charsWritten, HelperConst.BirthDayFormat, config.CultureInfo))
                {
                    CsvSerializeHelpers.EnsureBuffer(ref tmp);
                }
                CsvSerializeHelpers.WriteWithCheck(writer, tmp.AsSpan(), config, charsWritten);
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
    // If your target CPU is ARM, this value should be 256 and if the CPU is ARM64 or LOONGARCH64, it should be nuint.MaxValue/2. 
    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Buffer.Unix.cs
    internal const int BufferLength = 1024;

    internal const string BirthDayFormat = "yyyy年MM月dd日";
}
