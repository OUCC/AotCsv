#nullable enable
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Oucc.AotCsv.GeneratorHelpers;

namespace Oucc.AotCsv.ConsoleApp.GeneratedCodeTarget;

internal partial class @SampleModel : ICsvSerializable<@SampleModel>
{
    public static bool TryParseHeader(CsvParser parser, [NotNullWhen(true)] out int[]? columnMap)
    {
        var state = parser.TryGetLine(out var line);
        if (line.IsEmpty)
        {
            columnMap = null;
            return false;
        }

        var buffer = ArrayPool<int>.Shared.Rent(256);
        try
        {
            var leftIndex = 0;
            while (true)
            {
                if((leftIndex + 1 < line.Length && line[leftIndex + 1] == '"')
                switch (line)
                {
                    case "aa":
                        break;
                }
            }
        }
        finally
        {
            if (buffer is not null)
                ArrayPool<int>.Shared.Return(buffer);
        }

    }

    static bool ICsvSerializable<SampleModel>.TryParse(CsvParser parser, [NotNullWhen(true)] out @SampleModel? value)
    {
        int @Id = default;
        string? @FirstName = default;
        string? @MiddleName = default;
        string? @LastName = default;
        DateTime BirthDay = default;

        var state = parser.TryGetLine(out var line);

        int leftIndex = -1; // 左のコンマ 
        int columnIndex = 0;
        int targetIndex = parser.ColumnMap[columnIndex];

        for (; columnIndex < parser.ColumnCount; targetIndex = parser.ColumnMap[++columnIndex])
        {
            // " ありの場合
            if (leftIndex + 1 < line.Length && line[leftIndex + 1] == '"')
            {
                if (parser.Config.ReadQuote == ReadQuote.NoQuote || leftIndex + 2 >= line.Length)
                    goto returnLabel;

                var rightQuote = ParseHelpers.SearchRightQuoteInQuote(line[(leftIndex + 2)..]);

                if (rightQuote < 0 || line[rightQuote + 1] != ',')
                    goto returnLabel;
                var field = line[(leftIndex + 1)..rightQuote];

                switch (targetIndex)
                {
                    case 1:
                        if (!int.TryParse(field, parser.Config.CultureInfo, out Id))
                            goto returnLabel;
                        break;
                    case 2:
                        if (!ParseHelpers.TryParseString(field, true, out FirstName))
                            goto returnLabel;
                        break;
                    case 3:
                        if (!ParseHelpers.TryParseString(field, true, out LastName))
                            goto returnLabel;
                        break;
                    case 4:
                        if (!ParseHelpers.TryParseString(field, true, out LastName))
                            goto returnLabel;
                        break;
                    case 5:
                        if (!DateTime.TryParseExact(field, "yyyy年MM月dd日", parser.Config.CultureInfo, DateTimeStyles.None, out BirthDay))
                            goto returnLabel;
                        break;
                }

                leftIndex = rightQuote + 1;
            }
            else
            {
                var rightIndex = line[leftIndex..].IndexOf(',');

                if (parser.Config.ReadQuote == ReadQuote.HasQuote)
                    goto returnLabel;

                var field = rightIndex < 0 ? line[leftIndex..] : line[(leftIndex + 1)..(rightIndex - 1)];
                if (field.Contains('"'))
                    goto returnLabel;

                switch (targetIndex)
                {
                    case 1:
                        if (!int.TryParse(field, parser.Config.CultureInfo, out Id))
                            goto returnLabel;
                        break;
                    case 2:
                        FirstName = field.ToString();
                        break;
                    case 3:
                        MiddleName = field.ToString();
                        break;
                    case 4:
                        LastName = field.ToString();
                        break;
                    case 5:
                        if (!DateTime.TryParseExact(field, "yyyy年MM月dd日", parser.Config.CultureInfo, DateTimeStyles.None, out BirthDay))
                            goto returnLabel;
                        break;
                }

                leftIndex = rightIndex < 0 ? line.Length : rightIndex;
            }
        }

        value = new @SampleModel()
        {
            @Id = @Id!,
            @FirstName = @FirstName!,
            @MiddleName = @MiddleName!,
            @LastName = @LastName!,
            @BirthDay = BirthDay!,
        };
        return true;

returnLabel:
        value = null;
        return false;
    }

    static bool ICsvSerializable<SampleModel>.TryWrite(CsvWriter reader, SampleModel value)
    {
        var @a = value.MiddleName;
        throw new NotImplementedException();
    }
}
