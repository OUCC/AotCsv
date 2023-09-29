using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Oucc.AotCsv.Generator.Utility
{
    internal static class StringBuilderExtention
    {
        public static StringBuilder AppendFormatted(this StringBuilder builder, [InterpolatedStringHandlerArgument("builder")] in AppendInterpolatedStringHandler handler)
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

            public void AppendFormatted(string value) => _builder.Append(value);

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
        }
    }
}
