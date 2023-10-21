using Oucc.AotCsv.Test.TestTargets;
using Xunit;

namespace Oucc.AotCsv.Test;

public class DeserializePrimitiveTest
{
    private static void RunGenricsClassTest<T>(bool hasHeader, ReadQuote readQuote, string csv, T?[] expected) where T : struct, ISpanFormattable, ISpanParsable<T>
    {
        var config = CsvDeserializeConfig.Invariant with
        {
            ReadQuote = readQuote,
            HasHeader = hasHeader
        };

        using var sr = new StringReader(csv);
        using var csvr = new CsvReader(sr, config);
        var records = csvr.GetRecords<GenericsClass<T>>().ToArray();

        var record = Assert.Single(records);
        Assert.Equal(expected[0]!.Value, record.Property);
        Assert.Equal(default, record.ReadOnlyProperty);
        Assert.Equal(expected[1]!.Value, record.GetPrivateProperty);
        Assert.Equal(default, record.GetPrivateReadOnlyProperty);
        Assert.Equal(expected[2]!.Value, record.Field);
        Assert.Equal(expected[3]!.Value, record.ReadOnlyField);
        Assert.Equal(expected[4]!.Value, record.GetPrivateField);
        Assert.Equal(expected[5]!.Value, record.GetPrivateReadOnlyField);

        Assert.Equal(expected[6], record.NullableProperty);
        Assert.Null(record.NullableReadOnlyProperty);
        Assert.Equal(expected[7], record.GetNullablePrivateProperty);
        Assert.Null(record.GetNullablePrivateReadOnlyProperty);
        Assert.Equal(expected[8], record.NullableField);
        Assert.Equal(expected[9], record.NullableReadOnlyField);
        Assert.Equal(expected[10], record.GetNullablePrivateField);
        Assert.Equal(expected[11], record.GetNullablePrivateReadOnlyField);
    }

    private static void RunGenricsStructTest<T>(bool hasHeader, ReadQuote readQuote, string csv, T?[] expected) where T : struct, ISpanFormattable, ISpanParsable<T>
    {
        var config = CsvDeserializeConfig.Invariant with
        {
            ReadQuote = readQuote,
            HasHeader = hasHeader
        };

        using var sr = new StringReader(csv);
        using var csvr = new CsvReader(sr, config);
        var records = csvr.GetRecords<GenericsStruct<T>>().ToArray();

        var record = Assert.Single(records);
        Assert.Equal(expected[0]!.Value, record.Property);
        Assert.Equal(default, record.ReadOnlyProperty);
        Assert.Equal(expected[1]!.Value, record.GetPrivateProperty);
        Assert.Equal(default, record.GetPrivateReadOnlyProperty);
        Assert.Equal(expected[2]!.Value, record.Field);
        Assert.Equal(expected[3]!.Value, record.ReadOnlyField);
        Assert.Equal(expected[4]!.Value, record.GetPrivateField);
        Assert.Equal(expected[5]!.Value, record.GetPrivateReadOnlyField);

        Assert.Equal(expected[6], record.NullableProperty);
        Assert.Null(record.NullableReadOnlyProperty);
        Assert.Equal(expected[7], record.GetNullablePrivateProperty);
        Assert.Null(record.GetNullablePrivateReadOnlyProperty);
        Assert.Equal(expected[8], record.NullableField);
        Assert.Equal(expected[9], record.NullableReadOnlyField);
        Assert.Equal(expected[10], record.GetNullablePrivateField);
        Assert.Equal(expected[11], record.GetNullablePrivateReadOnlyField);
    }

    [Theory]
    [MemberData(nameof(GenericClassTest_Int_Data))]
    public void GenericClassTest_Int32(bool hasHeader, ReadQuote readQuote, string csv, int?[] expected)
    {
        RunGenricsClassTest(hasHeader, readQuote, csv, expected);
    }

    [Theory]
    [MemberData(nameof(GenericClassTest_Int_Data))]
    public void GenericStructTest_Int32(bool hasHeader, ReadQuote readQuote, string csv, int?[] expected)
    {
        RunGenricsStructTest(hasHeader, readQuote, csv, expected);
    }

    [Theory]
    [MemberData(nameof(GenericClassTest_Int_Data))]
    public void GenericClassTest_Int64(bool hasHeader, ReadQuote readQuote, string csv, int?[] expected)
    {
        RunGenricsClassTest(hasHeader, readQuote, csv, expected.Select(v => (long?)v).ToArray());
    }

    [Theory]
    [MemberData(nameof(GenericClassTest_Int_Data))]
    public void GenericStructTest_Int64(bool hasHeader, ReadQuote readQuote, string csv, int?[] expected)
    {
        RunGenricsStructTest(hasHeader, readQuote, csv, expected.Select(v => (long?)v).ToArray());
    }

    [Theory]
    [MemberData(nameof(GenericClassTest_Double_Data))]
    public void GenericClassTest_Double(bool hasHeader, ReadQuote readQuote, string csv, double?[] expected)
    {
        RunGenricsClassTest(hasHeader, readQuote, csv, expected);
    }

    [Theory]
    [MemberData(nameof(GenericClassTest_Double_Data))]
    public void GenericStructTest_Double(bool hasHeader, ReadQuote readQuote, string csv, double?[] expected)
    {
        RunGenricsStructTest(hasHeader, readQuote, csv, expected);
    }

    public static object[][] GenericClassTest_Int_Data()
    {
        var headers = new string[]
        {
            "Property", "PrivateProperty",
            "Field", "ReadOnlyField", "PrivateField", "PrivateReadOnlyField",
            "NullableProperty", "NullablePrivateProperty",
            "NullableField", "NullableReadOnlyField", "NullablePrivateField", "NullablePrivateReadOnlyField",
        };
        var expected = new int?[] { 1, 2, 3, 4, 5, 6, null, null, null, null, null, null };
        var values = expected.Select(x => x.ToString()).ToArray();
        (bool, ReadQuote, string, int?[])[] parameters =
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
            "NullableProperty", "NullablePrivateProperty",
            "NullableField", "NullableReadOnlyField", "NullablePrivateField", "NullablePrivateReadOnlyField",
        };
        var expected = new double?[] { 1.5, 0.25, 5, 0.125, 0.5, 10000, null, null, null, null, null, null };
        var values = expected.Select(x => x.ToString()).ToArray();
        (bool, ReadQuote, string, double?[])[] parameters =
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
