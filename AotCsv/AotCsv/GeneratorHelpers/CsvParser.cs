using System.Buffers;
using System.Collections.Immutable;
using System.Globalization;

namespace Oucc.AotCsv.GeneratorHelpers;

public class CsvParser
{
    /// <summary>
    /// new char[] で作成するボーダー
    /// これ以下の長さのときはArrayPoolを使わないほうが速い
    /// </summary>
    internal const int RawLength = 64;

    // 32768 byte
    private const int ReadBlock = 16384;
    private readonly TextReader _reader;
    public CsvDeserializeConfig Config { get; }

    private char[] _buffer;
    private int _bufferOffset;

    private int _bufferLength;
    /// <summary>
    /// 読み切ったかどうか
    /// </summary>
    private bool _isRead;

    public ImmutableArray<int> ColumnMap { get; } = default!;
    public int ColumnCount { get; }

    public CsvParser(TextReader reader, CsvDeserializeConfig config)
    {
        _buffer = ArrayPool<char>.Shared.Rent(ReadBlock);
        _reader = reader;
        Config = config;
    }

    public FieldState TryGetLine(out ReadOnlySpan<char> line) { throw new NotImplementedException(); }

    public ArrayContainer TryGetField(out FieldState fieldState)
    {
        var state = ReadingState.WaitingQuoteOrValue;
        Span<char> spanBuffer = _buffer.Length == _bufferOffset ? default : _buffer.AsSpan()[_bufferOffset.._bufferLength];


        var buffer = new char[RawLength];
        var destSpan = buffer.AsSpan();
        var length = 0;

        while (true)
        {

            for (var i = 0; i < spanBuffer.Length; i++)
            {
                var current = spanBuffer[i];
                switch (current)
                {
                    case '"':
                        switch (state)
                        {
                            case ReadingState.WaitingQuoteOrValue:
                                if (Config.ReadQuote == ReadQuote.NoQuote)
                                {
                                    AotCsvException.Throw();
                                    fieldState = FieldState.NoLine;
                                    return default;
                                }
                                state = ReadingState.ValueInQuote;
                                continue;
                            case ReadingState.ValueInQuote:
                                state = ReadingState.FirstQuoteInValue;
                                continue;
                            case ReadingState.FirstQuoteInValue:
                                EnsureBuffer(ref buffer, destSpan[..length], 1);
                                destSpan = buffer.AsSpan();
                                destSpan[length++] = '"';
                                state = ReadingState.ValueInQuote;
                                continue;
                            case ReadingState.AfterCR:
                                _bufferOffset += i;
                                fieldState = FieldState.LastField;
                                return new ArrayContainer(buffer, length);
                            default:
                                AotCsvException.Throw();
                                fieldState = FieldState.NoLine;
                                return default;
                        }
                    case ',':
                        switch (state)
                        {
                            case ReadingState.WaitingQuoteOrValue:
                                if (Config.ReadQuote == ReadQuote.HasQuote)
                                {
                                    AotCsvException.Throw();
                                    fieldState = FieldState.NoLine;
                                    return default;
                                }
                                _bufferOffset += i + 1;
                                fieldState = FieldState.HasField;
                                return default;
                            case ReadingState.ValueInQuote:
                                EnsureBuffer(ref buffer, destSpan[..length], 1);
                                destSpan = buffer.AsSpan();
                                destSpan[length++] = ',';
                                continue;
                            case ReadingState.ValueNotInQuote:
                                _bufferOffset += i + 1;
                                fieldState = FieldState.HasField;
                                return new ArrayContainer(buffer, length);
                            case ReadingState.AfterCR:
                                _bufferOffset += i;
                                fieldState = FieldState.LastField;
                                return new ArrayContainer(buffer, length);
                            case ReadingState.FirstQuoteInValue:
                                _bufferOffset += i + 1;
                                fieldState = FieldState.HasField;
                                return new ArrayContainer(buffer, length);
                            default:
                                AotCsvException.Throw();
                                continue;
                        }
                    case '\r':
                        switch (state)
                        {
                            case ReadingState.WaitingQuoteOrValue:
                                state = ReadingState.AfterCR;
                                continue;
                            case ReadingState.ValueInQuote:
                                EnsureBuffer(ref buffer, destSpan[..length], 1);
                                destSpan = buffer.AsSpan();
                                destSpan[length++] = '\r';
                                continue;
                            case ReadingState.ValueNotInQuote:
                                state = ReadingState.AfterCR;
                                continue;
                            case ReadingState.AfterCR:
                                _bufferOffset += i;
                                fieldState = FieldState.LastField;
                                return new ArrayContainer(buffer, length);
                            case ReadingState.FirstQuoteInValue:
                                state = ReadingState.AfterCR;
                                continue;
                            default:
                                AotCsvException.Throw();
                                fieldState = FieldState.NoLine;
                                return default;
                        }
                    case '\n':
                        switch (state)
                        {
                            case ReadingState.WaitingQuoteOrValue:
                                fieldState = FieldState.LastField;
                                return default;
                            case ReadingState.ValueInQuote:
                                EnsureBuffer(ref buffer, destSpan[..length], 1);
                                destSpan = buffer.AsSpan();
                                destSpan[length++] = '\n';
                                continue;
                            case ReadingState.ValueNotInQuote:
                                _bufferOffset += i + 1;
                                fieldState = FieldState.LastField;
                                return new ArrayContainer(buffer, length);
                            case ReadingState.FirstQuoteInValue:
                                _bufferOffset += i + 1;
                                fieldState = FieldState.LastField;
                                return new ArrayContainer(buffer, length);
                            case ReadingState.AfterCR:
                                _bufferOffset += i + 1;
                                fieldState = FieldState.LastField;
                                return new ArrayContainer(buffer, length);
                            default:
                                AotCsvException.Throw();
                                fieldState = FieldState.NoLine;
                                return default;
                        }
                    default:
                        switch (state)
                        {
                            case ReadingState.WaitingQuoteOrValue:
                                if (Config.ReadQuote == ReadQuote.HasQuote)
                                {
                                    AotCsvException.Throw();
                                    fieldState = FieldState.NoLine;
                                    return default;
                                }
                                state = ReadingState.ValueNotInQuote;
                                EnsureBuffer(ref buffer, destSpan[..length], 1);
                                destSpan = buffer.AsSpan();
                                destSpan[length++] = current;
                                continue;
                            case ReadingState.ValueNotInQuote:
                            case ReadingState.ValueInQuote:
                                EnsureBuffer(ref buffer, destSpan[..length], 1);
                                destSpan = buffer.AsSpan();
                                destSpan[length++] = current;
                                continue;
                            case ReadingState.FirstQuoteInValue:
                                AotCsvException.Throw();
                                fieldState = FieldState.NoLine;
                                return default;
                            case ReadingState.AfterCR:
                                _bufferOffset += i;
                                fieldState = FieldState.LastField;
                                return new ArrayContainer(buffer, length);
                            default:
                                AotCsvException.Throw();
                                fieldState = FieldState.NoLine;
                                return default;
                        }
                }
            }
            _bufferOffset += spanBuffer.Length;
            
            Read();

            if (_isRead &&  _bufferLength == 0)
            {
                fieldState = length != 0 ? FieldState.LastField: FieldState.NoLine;
                return new ArrayContainer(buffer, length);
            }
            spanBuffer = _buffer.Length == _bufferOffset ? default : _buffer.AsSpan()[_bufferOffset.._bufferLength];
        }
    }

    private void Read()
    {
        if (_isRead) return;

        Span<char> currentBuffer = _buffer.Length == _bufferOffset ? default : _buffer.AsSpan()[_bufferOffset.._bufferLength];
        if (_bufferOffset >= currentBuffer.Length && _bufferOffset != _buffer.Length && _buffer.Length < ReadBlock)
        {
            var span = _buffer.AsSpan();
            currentBuffer.CopyTo(span);

            var readLength = _reader.Read(span[currentBuffer.Length..]);
            _bufferLength = readLength + currentBuffer.Length;
            _bufferOffset = 0;
            _isRead = readLength == 0;
        }
        else if (currentBuffer.Length < ReadBlock / 4 * 3)
        {
            var nextBuffer = ArrayPool<char>.Shared.Rent(ReadBlock);
            var span = nextBuffer.AsSpan();

            currentBuffer.CopyTo(span);
            var readLength = _reader.Read(span[currentBuffer.Length..]);
            _bufferLength = currentBuffer.Length + readLength;

            ArrayPool<char>.Shared.Return(_buffer);
            _buffer = nextBuffer;
            _bufferOffset = 0;
            _isRead = readLength == 0;
        }
        else
        {
            var nextBuffer = ArrayPool<char>.Shared.Rent(currentBuffer.Length + ReadBlock);
            var span = nextBuffer.AsSpan();

            currentBuffer.CopyTo(span);
            var readLength = _reader.Read(span[currentBuffer.Length..]);
            _bufferLength = currentBuffer.Length + readLength;

            ArrayPool<char>.Shared.Return(_buffer);
            _buffer = nextBuffer;
            _bufferOffset = 0;
            _isRead = readLength == 0;
        }
    }

    private void EnsureBuffer(ref char[] buffer, Span<char> contentBuffer, int additionalLength)
    {
        var requiredLength = contentBuffer.Length + additionalLength;
        if (buffer.Length > requiredLength) return;

        var temp = ArrayPool<char>.Shared.Rent(requiredLength);
        contentBuffer.CopyTo(temp);
        ArrayPool<char>.Shared.Return(buffer);
        buffer = temp;
    }

    private enum ReadingState
    {
        WaitingQuoteOrValue,
        ValueNotInQuote,
        ValueInQuote,
        FirstQuoteInValue,
        AfterCR,
    }
}
