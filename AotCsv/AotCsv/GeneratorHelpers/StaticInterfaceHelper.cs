using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Oucc.AotCsv.GeneratorHelpers;

public static class StaticInterfaceHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse<T>(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out T result) where T : ISpanParsable<T>
        => T.TryParse(s, provider, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFormat<T>(T value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) where T : ISpanFormattable
        => value.TryFormat(destination, out charsWritten, format, provider);
}
