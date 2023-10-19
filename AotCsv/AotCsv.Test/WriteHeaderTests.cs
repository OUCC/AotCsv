using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oucc.AotCsv.GeneratorHelpers;
using Oucc.AotCsv.Test.TestTargets;
using Oucc.AotCsv.Test.Utility;
using Xunit;

namespace Oucc.AotCsv.Test
{
    public class WriteHeaderTests
    {
        private static readonly string _mustNoQuoteOrShouldQuoteWithoutAttributesExpected = $"""
            FieldBoolean,FieldBooleanNullable,FieldByte,FieldByteNullable,FieldSByte,FieldSByteNullable,FieldDecimal,FieldDecimalNullable,FieldDouble,FieldDoubleNullable,FieldFloat,FieldFloatNullable,FieldInt16,FieldInt16Nullable,FieldUInt16,FieldUInt16Nullable,FieldInt32,FieldInt32Nullable,FieldUInt32,FieldUInt32Nullable,FieldIntPtr,FieldIntPtrNullable,FieldUIntPtr,FieldUIntPtrNullable,FieldInt64,FieldInt64Nullable,FieldUInt64,FieldUInt64Nullable,FieldChar,FieldCharNullable,FieldString,FieldStringNullable{Environment.NewLine}
            """;
        private static readonly string _mustNoQuoteOrShouldQuoteWithAttributesExpected = $"""
            TestDateTime?,TestDateTime,TestInt{Environment.NewLine}
            """;

        public static readonly object[][] WithoutAttributesTestSource = new object[][]
        {
            new object[] { _mustNoQuoteOrShouldQuoteWithoutAttributesExpected, QuoteOption.MustNoQuote },
            new object[] { $"""
                "FieldBoolean","FieldBooleanNullable","FieldByte","FieldByteNullable","FieldSByte","FieldSByteNullable","FieldDecimal","FieldDecimalNullable","FieldDouble","FieldDoubleNullable","FieldFloat","FieldFloatNullable","FieldInt16","FieldInt16Nullable","FieldUInt16","FieldUInt16Nullable","FieldInt32","FieldInt32Nullable","FieldUInt32","FieldUInt32Nullable","FieldIntPtr","FieldIntPtrNullable","FieldUIntPtr","FieldUIntPtrNullable","FieldInt64","FieldInt64Nullable","FieldUInt64","FieldUInt64Nullable","FieldChar","FieldCharNullable","FieldString","FieldStringNullable"{Environment.NewLine}
                """ , QuoteOption.MustQuote },
            new object[] { _mustNoQuoteOrShouldQuoteWithoutAttributesExpected, QuoteOption.ShouldQuote }
        };

        public static readonly object[][] WithAttributesTestSource = new object[][]
        {
            new object[] { _mustNoQuoteOrShouldQuoteWithAttributesExpected, QuoteOption.MustNoQuote },
            new object[] { $"""
                "TestDateTime?","TestDateTime","TestInt"{Environment.NewLine}
                """, QuoteOption.MustQuote },
            new object[] { _mustNoQuoteOrShouldQuoteWithAttributesExpected, QuoteOption.ShouldQuote },
        };

        [Theory]
        [MemberData(nameof(WithoutAttributesTestSource))]
        public void WithoutAttributesTest(string expected, QuoteOption option)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
            var config = new CsvSerializeConfig(option, CultureInfo.InvariantCulture);
            using var csvWriter = new CsvWriter<FieldOnlyClass>(writer, config);
            csvWriter.WriteHeader();
            writer.Flush();
            var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
            writer.Close();

            Assert.Equal(expected, csvText);
        }

        [Theory]
        [MemberData(nameof(WithAttributesTestSource))]
        public void WithAttributesTest(string expected, QuoteOption option)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
            var config = new CsvSerializeConfig(option, CultureInfo.InvariantCulture);
            using var csvWriter = new CsvWriter<SampleClass>(writer, config);
            csvWriter.WriteHeader();
            writer.Flush();
            var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
            writer.Close();

            Assert.Equal(expected, csvText);
        }

    }
}
