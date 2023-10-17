using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Oucc.AotCsv.Generator.Utility
{
    internal static class StringBuilderExtention
    {
        /// <summary>
        /// bool のみ通常のものと結果が異なります。小文字になります。
        /// </summary>
        public static StringBuilder AppendFormatted(this StringBuilder builder, [InterpolatedStringHandlerArgument("builder")] in AppendInterpolatedStringHandler _)
        {
            return builder;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [InterpolatedStringHandler]
        internal readonly struct AppendInterpolatedStringHandler
        {
            private readonly StringBuilder _builder;

            public AppendInterpolatedStringHandler(int litralLength, int formattedCount, StringBuilder builder)
            {
                builder.EnsureCapacity(builder.Length + litralLength + formattedCount * 4);
                _builder = builder;
            }

            public void AppendLiteral(string value) => _builder.Append(value);

            public void AppendFormatted(string? value) => _builder.Append(value);

            public void AppendFormatted(bool value) => _ = value ? _builder.Append("true") : _builder.Append("false");

            public void AppendFormatted(sbyte value) => _builder.Append(value);

            public void AppendFormatted(byte value) => _builder.Append(value);

            public void AppendFormatted(short value) => _builder.Append(value);

            public void AppendFormatted(int value) => _builder.Append(value);

            public void AppendFormatted(long value) => _builder.Append(value);

            public void AppendFormatted(float value) => _builder.Append(value);

            public void AppendFormatted(double value) => _builder.Append(value);

            public void AppendFormatted(decimal value) => _builder.Append(value);

            public void AppendFormatted(ushort value) => _builder.Append(value);

            public void AppendFormatted(uint value) => _builder.Append(value);

            public void AppendFormatted(ulong value) => _builder.Append(value);

            public void AppendFormatted(bool? value)
            {
                if (value.HasValue) AppendFormatted(value.Value);
                else _builder.Append("null");
            }

            public void AppendFormatted(sbyte? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");

            public void AppendFormatted(byte? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");

            public void AppendFormatted(short? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");

            public void AppendFormatted(int? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");

            public void AppendFormatted(long? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");

            public void AppendFormatted(float? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");

            public void AppendFormatted(double? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");

            public void AppendFormatted(decimal? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");

            public void AppendFormatted(ushort? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");

            public void AppendFormatted(uint? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");

            public void AppendFormatted(ulong? value) => _ = value.HasValue ? _builder.Append(value.Value) : _builder.Append("null");
        }
    }
}
