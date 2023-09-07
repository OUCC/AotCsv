using System.Buffers;
using System.Collections.Immutable;
using System.Globalization;

namespace Oucc.AotCsv.GeneratorHelpers;

public class CsvParser
{
    // 32768 byte
    private const int ReadBlock = 16384;
    private readonly TextReader _reader;
    public CsvDeserializeConfig Config { get; }

    private char[] _buffer;
    private int _bufferOffset;
    private int _bufferLength;

    public ImmutableArray<int> ColumnMap { get; } = default!;
    public int ColumnCount { get; }

    internal CsvParser(TextReader reader, CsvDeserializeConfig config)
    {
        _buffer = ArrayPool<char>.Shared.Rent(ReadBlock);
        _reader = reader;
        Config = config;
    }

    public FieldState TryGetLine(out ReadOnlySpan<char> line)
    {
        Span<char> spanBuffer = _buffer.Length == _bufferOffset ? default : _buffer.AsSpan()[_bufferOffset.._bufferLength];

        var endLinePosition = spanBuffer.IndexOfAny('\r', '\n');
        var noLine = false;
        while (endLinePosition < 0)
        {
            noLine = !TryRead();
            spanBuffer = _buffer.AsSpan()[_bufferOffset.._bufferLength];
            endLinePosition = spanBuffer.IndexOfAny('\r', '\n');
        }

        if (spanBuffer[endLinePosition] == '\r')
        {
            if (spanBuffer.Length == endLinePosition + 1)
            {
                noLine = !TryRead();
                spanBuffer = _buffer.AsSpan()[_bufferOffset.._bufferLength];
            }

            if (spanBuffer[endLinePosition + 1] == '\n') _bufferOffset += endLinePosition + 2;
            else _bufferOffset += endLinePosition + 1;
        }
        else _bufferOffset += endLinePosition + 1;

        line = spanBuffer[..endLinePosition];
        return FieldState.HasField;
    }

    private bool TryRead()
    {
        Span<char> currentBuffer = _buffer.Length == _bufferOffset ? default : _buffer.AsSpan()[_bufferOffset.._bufferLength];
        if (_bufferOffset >= currentBuffer.Length && _bufferOffset != _buffer.Length && _buffer.Length < ReadBlock)
        {
            var span = _buffer.AsSpan();
            currentBuffer.CopyTo(span);

            var readLength = _reader.Read(span[currentBuffer.Length..]);
            _bufferLength = readLength + currentBuffer.Length;
            _bufferOffset = 0;
            return readLength != 0;
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
            return readLength != 0;
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
            return readLength != 0;
        }
    }
}
