using Oucc.AotCsv.Test.TestTargets;
using Xunit;

namespace Oucc.AotCsv.Test;

public class DeserializeTest
{
    [Theory]
    [MemberData(nameof(GenericClassTest_Int_Data))]
    public void GenericClassTest_Int32(bool hasHeader, ReadQuote readQuote, string csv, int[] expected)
    {
        var config = CsvDeserializeConfig.Invariant with
        {
            ReadQuote = readQuote,
            HasHeader = hasHeader
        };

        using var sr = new StringReader(csv);
        using var csvr = new CsvReader(sr, config);
        var records = csvr.GetRecords<GenericsClass<int>>().ToArray();

        var record = Assert.Single(records);
        Assert.Equal(expected[0], record.Property);
        Assert.Equal(0, record.ReadOnlyProperty);
        Assert.Equal(expected[1], record.GetPrivateProperty);
        Assert.Equal(0, record.GetPrivateReadOnlyProperty);
        Assert.Equal(expected[2], record.Field);
        Assert.Equal(expected[3], record.ReadOnlyField);
        Assert.Equal(expected[4], record.GetPrivateField);
        Assert.Equal(expected[5], record.GetPrivateReadOnlyField);
    }

    [Theory]
    [MemberData(nameof(GenericClassTest_Int_Data))]
    public void GenericClassTest_Int64(bool hasHeader, ReadQuote readQuote, string csv, int[] expected)
    {
        var config = CsvDeserializeConfig.Invariant with
        {
            ReadQuote = readQuote,
            HasHeader = hasHeader
        };

        using var sr = new StringReader(csv);
        using var csvr = new CsvReader(sr, config);
        var records = csvr.GetRecords<GenericsClass<long>>().ToArray();

        var record = Assert.Single(records);
        Assert.Equal(expected[0], record.Property);
        Assert.Equal(0, record.ReadOnlyProperty);
        Assert.Equal(expected[1], record.GetPrivateProperty);
        Assert.Equal(0, record.GetPrivateReadOnlyProperty);
        Assert.Equal(expected[2], record.Field);
        Assert.Equal(expected[3], record.ReadOnlyField);
        Assert.Equal(expected[4], record.GetPrivateField);
        Assert.Equal(expected[5], record.GetPrivateReadOnlyField);
    }

    [Theory]
    [MemberData(nameof(GenericClassTest_Double_Data))]
    public void GenericClassTest_Double(bool hasHeader, ReadQuote readQuote, string csv, double[] expected)
    {
        var config = CsvDeserializeConfig.Invariant with
        {
            ReadQuote = readQuote,
            HasHeader = hasHeader
        };

        using var sr = new StringReader(csv);
        using var csvr = new CsvReader(sr, config);
        var records = csvr.GetRecords<GenericsClass<double>>().ToArray();

        var record = Assert.Single(records);
        Assert.Equal(expected[0], record.Property);
        Assert.Equal(0, record.ReadOnlyProperty);
        Assert.Equal(expected[1], record.GetPrivateProperty);
        Assert.Equal(0, record.GetPrivateReadOnlyProperty);
        Assert.Equal(expected[2], record.Field);
        Assert.Equal(expected[3], record.ReadOnlyField);
        Assert.Equal(expected[4], record.GetPrivateField);
        Assert.Equal(expected[5], record.GetPrivateReadOnlyField);
    }

    public static object[][] GenericClassTest_Int_Data()
    {
        var headers = new string[]
        {
            "Property", "PrivateProperty",
            "Field", "ReadOnlyField", "PrivateField", "PrivateReadOnlyField",
        };
        var expected = new int[] { 1, 2, 3, 4, 5, 6 };
        var values = expected.Select(x => x.ToString()).ToArray();
        (bool, ReadQuote, string, int[])[] parameters =
        {
            (true, ReadQuote.Auto, $"{string.Join(",", headers)}\n{string.Join(",", values)}", expected),
            (true, ReadQuote.Auto, $"\"{string.Join("\",\"", headers)}\"\n\"{string.Join("\",\"", values)}\"", expected),
            (true, ReadQuote.NoQuote, $"{string.Join(",", headers)}\n{string.Join(",", values)}", expected),
            (true, ReadQuote.HasQuote, $"\"{string.Join("\",\"", headers)}\"\n\"{string.Join("\",\"", values)}\"", expected),
            (false, ReadQuote.Auto , $"{string.Join(",", values)}" ,expected),
            (false, ReadQuote.Auto, $"\"{string.Join("\",\"", values)}\"", expected),
            (false, ReadQuote.NoQuote, $"{string.Join(",", values)}", expected),
            (false, ReadQuote.HasQuote, $"\"{string.Join("\",\"", values)}\"", expected),
        };
        return parameters.Select(p => new object[] { p.Item1, p.Item2, p.Item3, p.Item4 }).ToArray();
    }

    public static object[][] GenericClassTest_Double_Data()
    {
        var headers = new string[]
        {
            "Property", "PrivateProperty",
            "Field", "ReadOnlyField", "PrivateField", "PrivateReadOnlyField",
        };
        var expected = new double[] { 1.5, 0.25, 5, 0.125, 0.5, 10000 };
        var values = expected.Select(x => x.ToString()).ToArray();
        (bool, ReadQuote, string, double[])[] parameters =
        {
            (true, ReadQuote.Auto, $"{string.Join(",", headers)}\n{string.Join(",", values)}", expected),
            (true, ReadQuote.Auto, $"\"{string.Join("\",\"", headers)}\"\n\"{string.Join("\",\"", values)}\"", expected),
            (true, ReadQuote.NoQuote, $"{string.Join(",", headers)}\n{string.Join(",", values)}", expected),
            (true, ReadQuote.HasQuote, $"\"{string.Join("\",\"", headers)}\"\n\"{string.Join("\",\"", values)}\"", expected),
            (false, ReadQuote.Auto , $"{string.Join(",", values)}" ,expected),
            (false, ReadQuote.Auto, $"\"{string.Join("\",\"", values)}\"", expected),
            (false, ReadQuote.NoQuote, $"{string.Join(",", values)}", expected),
            (false, ReadQuote.HasQuote, $"\"{string.Join("\",\"", values)}\"", expected),
        };
        return parameters.Select(p => new object[] { p.Item1, p.Item2, p.Item3, p.Item4 }).ToArray();
    }
}
