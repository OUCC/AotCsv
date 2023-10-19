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
        [Fact]
        public void MustNoQuoteWithoutAttributesTest()
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
            var config = new CsvSerializeConfig(QuoteOption.MustNoQuote, CultureInfo.InvariantCulture);
            using var csvWriter = new CsvWriter<FieldOnlyClass>(writer, config);
            csvWriter.WriteHeader();
            writer.Flush();
            var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
            writer.Close();

            Assert.Equal(SerializeTestHelper.MustNoQuoteOrShouldQuoteWithoutAttributesExpected,csvText);
        }

        [Fact]
        public void MustQuoteWithoutAttributesTest()
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
            var config = new CsvSerializeConfig(QuoteOption.MustQuote, CultureInfo.InvariantCulture);
            using var csvWriter = new CsvWriter<FieldOnlyClass>(writer, config);
            csvWriter.WriteHeader();
            writer.Flush();
            var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
            writer.Close();

            Assert.Equal(SerializeTestHelper.MustQuoteWithoutAttributesExpected,csvText);
        }

        [Fact]
        public void ShouldQuoteWithoutAttributesTest()
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
            var config = new CsvSerializeConfig(QuoteOption.ShouldQuote, CultureInfo.InvariantCulture);
            using var csvWriter = new CsvWriter<FieldOnlyClass>(writer, config);
            csvWriter.WriteHeader();
            writer.Flush();
            var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
            writer.Close();

            Assert.Equal(SerializeTestHelper.MustNoQuoteOrShouldQuoteWithoutAttributesExpected,csvText);
        }

        [Fact]
        public void MustNoQuoteWithAttributesTest()
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
            var config = new CsvSerializeConfig(QuoteOption.MustNoQuote, CultureInfo.InvariantCulture);
            using var csvWriter = new CsvWriter<SampleClass>(writer, config);
            csvWriter.WriteHeader();
            writer.Flush();
            var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
            writer.Close();

            Assert.Equal(SerializeTestHelper.MustNoQuoteOrShouldQuoteWithAttributesExpected,csvText);
        }

        [Fact]
        public void MustQuoteWithAttributesTest()
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
            var config = new CsvSerializeConfig(QuoteOption.MustQuote, CultureInfo.InvariantCulture);
            using var csvWriter = new CsvWriter<SampleClass>(writer, config);
            csvWriter.WriteHeader();
            writer.Flush();
            var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
            writer.Close();

            Assert.Equal(SerializeTestHelper.MustQuoteWithAttributesExpected,csvText);
        }

        [Fact]
        public void ShouldQuoteWithAttributesTest()
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
            var config = new CsvSerializeConfig(QuoteOption.MustNoQuote, CultureInfo.InvariantCulture);
            using var csvWriter = new CsvWriter<SampleClass>(writer, config);
            csvWriter.WriteHeader();
            writer.Flush();
            var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
            writer.Close();

            Assert.Equal(SerializeTestHelper.MustNoQuoteOrShouldQuoteWithAttributesExpected,csvText);
        }
    }
}
