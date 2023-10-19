using Xunit;
using System.Text;
using System.Globalization;
using Oucc.AotCsv.GeneratorHelpers;
using Oucc.AotCsv.Test.TestTargets;
using Oucc.AotCsv.Test.Utility;

namespace Oucc.AotCsv.Test;

public partial class WriteRecordsTests
{

    [Fact]
    internal void MustNoQuoteAndFieldOnlyClassTest()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(QuoteOption.MustNoQuote, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<FieldOnlyClass>(writer, config);
        csvWriter.WriteRecords(SerializeTestHelper.FieldOnlyClassList);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(SerializeTestHelper.MustNoQuoteExpected, csvText);
    }

    [Fact]
    internal void MustQuoteAndFieldOnlyClassTest()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(QuoteOption.MustQuote, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<FieldOnlyClass>(writer, config);
        csvWriter.WriteRecords(SerializeTestHelper.FieldOnlyClassList);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(SerializeTestHelper.MustQuoteExpected, csvText);
    }

    [Fact]
    internal void ShouldQuoteAndFieldOnlyClassTest()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(QuoteOption.ShouldQuote, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<FieldOnlyClass>(writer, config);
        csvWriter.WriteRecords(SerializeTestHelper.FieldOnlyClassListForShouldQuote);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(SerializeTestHelper.ShouldQuoteExpected, csvText);
    }

    [Fact]
    internal void MustNoQuoteAndPropertyOnlyClassTest()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(QuoteOption.MustNoQuote, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<PropertyOnlyClass>(writer, config);
        csvWriter.WriteRecords(SerializeTestHelper.PropertyOnlyClassList);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(SerializeTestHelper.MustNoQuoteExpected, csvText);
    }

    [Fact]
    internal void MustQuoteAndPropertyOnlyClassTest()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(QuoteOption.MustQuote, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<PropertyOnlyClass>(writer, config);
        csvWriter.WriteRecords(SerializeTestHelper.PropertyOnlyClassList);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(SerializeTestHelper.MustQuoteExpected, csvText);
    }

    [Fact]
    internal void ShouldQuoteAndPropertyOnlyClassTest()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(QuoteOption.ShouldQuote, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<PropertyOnlyClass>(writer, config);
        csvWriter.WriteRecords(SerializeTestHelper.PropertyOnlyClassListForShouldQuote);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(SerializeTestHelper.ShouldQuoteExpected, csvText);
    }

    [Fact]
    internal void MustNoQuoteSampleClassTest()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(QuoteOption.MustNoQuote, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<SampleClass>(writer, config);
        csvWriter.WriteRecords(SerializeTestHelper.SampleClass1List);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(SerializeTestHelper.ShouldOrMustNoQuoteSampleClassExpected, csvText);
    }

    [Fact]
    internal void MustQuoteSampleClassTest()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(QuoteOption.MustQuote, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<SampleClass>(writer, config);
        csvWriter.WriteRecords(SerializeTestHelper.SampleClass1List);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(SerializeTestHelper.MustQuoteSampleClassExpected, csvText);
    }

    [Fact]
    internal void ShouldQuoteSampleClassTest()
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(QuoteOption.ShouldQuote, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<SampleClass>(writer, config);
        csvWriter.WriteRecords(SerializeTestHelper.SampleClass1List);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(SerializeTestHelper.ShouldOrMustNoQuoteSampleClassExpected, csvText);
    }
}
