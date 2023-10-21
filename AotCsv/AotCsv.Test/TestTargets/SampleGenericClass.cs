using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oucc.AotCsv.Attributes;

namespace Oucc.AotCsv.Test.TestTargets
{
    [CsvSerializable]
    internal partial class StructGenericClass<T1>
        where T1 : struct, ISpanParsable<T1>, ISpanFormattable
    {
        public T1 T1IntProperty { get; set; } = default;
    }

    [CsvSerializable]
    internal partial class ClassGenericClass<T1>
        where T1 : class, ISpanFormattable, ISpanParsable<T1>, new()
    {
        public T1 T1FormattableParsableClassProperty { get; set; } = new();
    }

    [CsvSerializable]
    internal partial class SampleGenericClass<T1, T2, T3>
        where T1 : notnull, ISpanParsable<T1>, ISpanFormattable
        where T2 : notnull, ISpanFormattable, ISpanParsable<T2>
        where T3 : notnull, ISpanFormattable, ISpanParsable<T3>
    {
        [CsvName("bool")]
        public bool BoolProperty { get; set; }
        [CsvName("T1")]
        public T1? T1Field = default;
        [CsvName("T2")]
        public T2? T2Field = default;
        [CsvName("T3?")]
        public T3? T3FormattableParsableClassFieldNullable = default;
    }

    internal class FormattableAndParsableClass : ISpanParsable<FormattableAndParsableClass>, ISpanFormattable
    {
        internal string Field;
        public static FormattableAndParsableClass Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static FormattableAndParsableClass Parse(string s, IFormatProvider? provider)
        {
            throw new NotImplementedException();
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out FormattableAndParsableClass result)
        {
            result = new FormattableAndParsableClass(s.ToString());
            return true;
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out FormattableAndParsableClass result)
        {
            if (s is not null)
            {
                result = new FormattableAndParsableClass(s);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return Field;
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            charsWritten = Field.Length;
            return Field.TryCopyTo(destination);
        }

        public FormattableAndParsableClass(string field)
        {
            Field = field;
        }

        public FormattableAndParsableClass() : this("FormattableParsableClass") { }
    }
}
