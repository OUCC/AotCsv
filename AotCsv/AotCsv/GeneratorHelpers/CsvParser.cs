using System.Buffers;
using System.Globalization;

namespace Oucc.AotCsv.GeneratorHelpers;

public class CsvParser
{
    // 131072 byte
    private const int ReadBlock = 65536;
    private readonly TextReader _reader;
    private readonly CultureInfo _culture;

    private char[]? _buffer;
    private int _bufferOffset;
    private int _bufferLength;

    internal CsvParser(TextReader reader, CultureInfo culture)
    {
        _reader = reader;
        _culture = culture;
    }

    public FieldState TryGetLine(out ReadOnlySpan<char> line)
    {
        Span<char> spanBuffer;
        if (_buffer is null)
        {
            spanBuffer = _buffer = ArrayPool<char>.Shared.Rent(ReadBlock);
            _bufferOffset = 0;
            _bufferLength = _reader.Read(spanBuffer);
            if (_bufferLength == 0)
            {
                line = default;
                return FieldState.NoLine;
            }
            spanBuffer = spanBuffer[.._bufferLength];
        }
        else
        {
            spanBuffer = _buffer.AsSpan()[_bufferOffset.._bufferLength];
        }

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
        if (_buffer is null)
        {
            Span<char> spanBuffer = _buffer = ArrayPool<char>.Shared.Rent(ReadBlock);
            _bufferOffset = 0;
            _bufferLength = _reader.Read(spanBuffer);

            return _bufferLength != 0;
        }
        else
        {
            Span<char> currentBuffer = _buffer.AsSpan()[_bufferOffset.._bufferLength];
            if (currentBuffer.Length < ReadBlock)
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

}
