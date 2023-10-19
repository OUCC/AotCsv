using Xunit;
using System.Text;
using System.Globalization;
using Oucc.AotCsv.GeneratorHelpers;
using Oucc.AotCsv.Test.TestTargets;
using Oucc.AotCsv.Test.Utility;

namespace Oucc.AotCsv.Test;

public partial class WriteRecordsTests
{

    private static readonly DateTime _testTime = new(2023, 1, 1);
    private static readonly DateTime _utcNow = DateTime.UtcNow;
    private static readonly string _mustNoQuoteExpected = $"False,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,{'\0'},,,{Environment.NewLine}True,False,1,0,1,0,10.25,10,130.123,530.25,354,255.2,-16,-160,16,160,-32,-320,32,320,-3264,-32640,3264,32640,-64,-640,64,640,a,n,normal,not null{Environment.NewLine}";
    private static readonly string _mustQuoteExpected = $"""
            "False","","0","","0","","0","","0","","0","","0","","0","","0","","0","","0","","0","","0","","0","","{'\0'}","","",""{Environment.NewLine}"True","False","1","0","1","0","10.25","10","130.123","530.25","354","255.2","-16","-160","16","160","-32","-320","32","320","-3264","-32640","3264","32640","-64","-640","64","640","a","n","normal","not null"{Environment.NewLine}
            """;
    private static readonly string _shouldQuoteExpected = $"""""
            False,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,"""","{'\n'}","Test """" Quotes{"\r\n"} Test "",","Test {'\r'} only"{Environment.NewLine}
            """"";
    private static readonly string _shouldOrMustNoQuoteSampleClassExpected = $"""
            ,{_testTime.ToString("MMMM dd dddd", CultureInfo.InvariantCulture)},0{Environment.NewLine}{_utcNow.ToString("HH mm ss", CultureInfo.InvariantCulture)},{_testTime.ToString("MMMM dd dddd", CultureInfo.InvariantCulture)},100{Environment.NewLine}
            """;
    private static readonly string _mustQuoteSampleClassExpected = $"""
            "","{_testTime.ToString("MMMM dd dddd", CultureInfo.InvariantCulture)}","0"{Environment.NewLine}"{_utcNow.ToString("HH mm ss", CultureInfo.InvariantCulture)}","{_testTime.ToString("MMMM dd dddd", CultureInfo.InvariantCulture)}","100"{Environment.NewLine}
            """;
    private static readonly List<FieldOnlyClass> _fieldOnlyClassList = new() {
            new FieldOnlyClass(),
            new FieldOnlyClass(true, false, 1, 0, 1, 0, 10.25m, 10m, 130.123, 530.25, 354f, 255.2f, -16, -160, 16, 160, -32, -320, 32, 320, -3264, -32640, 3264, 32640, -64, -640, 64, 640, 'a', 'n', "normal", "not null")
        };
    private static readonly List<FieldOnlyClass> _fieldOnlyClassListForShouldQuote = new() {
            new FieldOnlyClass(@char: '\"', charNullable: '\n', @string: "Test \"\" Quotes\r\n Test \",", stringNullable: "Test \r only")
        };
    private static readonly List<PropertyOnlyClass> _propertyOnlyClassList = new() {
            new PropertyOnlyClass(),
            new PropertyOnlyClass(true, false, 1, 0, 1, 0, 10.25m, 10m, 130.123, 530.25, 354f, 255.2f, -16, -160, 16, 160, -32, -320, 32, 320, -3264, -32640, 3264, 32640, -64, -640, 64, 640, 'a', 'n', "normal", "not null")
        };
    private static readonly List<PropertyOnlyClass> _propertyOnlyClassListForShouldQuote = new() {
            new PropertyOnlyClass(@char: '\"', charNullable: '\n', @string: "Test \"\" Quotes\r\n Test \",", stringNullable: "Test \r only")
        };
    private static readonly List<SampleClass> _sampleClassList = new()
        {
            new SampleClass(0, _testTime, null),
            new SampleClass(100, _testTime, _utcNow)
        };

    public static readonly object[][] FieldOnlyClassTestSource = new object[][]
    {
        new object[]{ _mustNoQuoteExpected, QuoteOption.MustNoQuote, _fieldOnlyClassList },
        new object[]{ _mustQuoteExpected, QuoteOption.MustQuote, _fieldOnlyClassList },
        new object[]{ _shouldQuoteExpected, QuoteOption.ShouldQuote, _fieldOnlyClassListForShouldQuote }
    };

    public static readonly object[][] PropertyOnlyClassTestSource = new object[][]
    {
        new object[]{ _mustNoQuoteExpected, QuoteOption.MustNoQuote, _propertyOnlyClassList },
        new object[]{ _mustQuoteExpected, QuoteOption.MustQuote, _propertyOnlyClassList },
        new object[]{ _shouldQuoteExpected, QuoteOption.ShouldQuote, _propertyOnlyClassListForShouldQuote }
    };

    public static readonly object[][] SampleClassTestSource = new object[][]
    {
        new object[] { _shouldOrMustNoQuoteSampleClassExpected, QuoteOption.MustNoQuote, _sampleClassList },
        new object[] { _mustQuoteSampleClassExpected, QuoteOption.MustQuote, _sampleClassList },
        new object[] { _shouldOrMustNoQuoteSampleClassExpected, QuoteOption.ShouldQuote, _sampleClassList }
    };

    [Theory]
    [MemberData(nameof(FieldOnlyClassTestSource))]
    internal void FieldOnlyClassTest(string expected, QuoteOption option, List<FieldOnlyClass> list)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(option, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<FieldOnlyClass>(writer, config);
        csvWriter.WriteRecords(list);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(expected, csvText);
    }

    [Theory]
    [MemberData(nameof(PropertyOnlyClassTestSource))]
    internal void PropertyOnlyClassTest(string expected, QuoteOption option, List<PropertyOnlyClass> list)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(option, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<PropertyOnlyClass>(writer, config);
        csvWriter.WriteRecords(list);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(expected, csvText);
    }

    [Theory]
    [MemberData(nameof(SampleClassTestSource))]
    internal void SampleClassTest(string expected, QuoteOption option, List<SampleClass> list)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, SerializeTestHelper.UnicodeNoBOM);
        var config = new CsvSerializeConfig(option, CultureInfo.InvariantCulture);
        using var csvWriter = new CsvWriter<SampleClass>(writer, config);
        csvWriter.WriteRecords(list);
        writer.Flush();
        var csvText = SerializeTestHelper.UnicodeNoBOM.GetString(ms.ToArray());
        writer.Close();

        Assert.Equal(expected, csvText);
    }
}
