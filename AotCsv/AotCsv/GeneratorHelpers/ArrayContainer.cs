using System.Buffers;

namespace Oucc.AotCsv.GeneratorHelpers;

public readonly ref struct ArrayContainer
{
    private readonly char[]? _destination;

    private readonly int _length;

    internal ArrayContainer(char[] destination, int length)
    {
        _destination = destination;
        _length = length;
    }

    public readonly ReadOnlySpan<char> AsSpan() => _destination is null ? default : _destination.AsSpan(0, _length);

    public readonly void Dispose()
    {
        if (_destination is not null && _destination.Length > CsvParser.RawLength)
        {
            ArrayPool<char>.Shared.Return(_destination);
        }
    }
}
