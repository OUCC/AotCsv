﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Oucc.AotCsv.GeneratorHelpers;

public static class StaticInterfaceHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse<T>(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out T result) where T : ISpanParsable<T>
        => T.TryParse(s, provider, out result);
}
