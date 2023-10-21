using Oucc.AotCsv.Attributes;

namespace Oucc.AotCsv.Test.TestTargets;

[CsvSerializable]
internal partial class GenericsStruct<T> where T : struct, ISpanFormattable, ISpanParsable<T>
{
    public T Property { get; set; }

    [CsvInclude]
    public T ReadOnlyProperty { get; }

    [CsvInclude]
    private T PrivateProperty { get; set; }

    [CsvInclude]
    private T PrivateReadOnlyProperty { get; }

    public T Field;

    [CsvInclude]
    public T ReadOnlyField;

    [CsvInclude]
    private T PrivateField;

    [CsvInclude]
    private T PrivateReadOnlyField;

    public T GetPrivateProperty => PrivateProperty;

    public T GetPrivateReadOnlyProperty => PrivateReadOnlyProperty;

    public T GetPrivateField => PrivateField;

    public T GetPrivateReadOnlyField => PrivateReadOnlyField;

    public T? NullableProperty { get; set; }

    [CsvInclude]
    public T? NullableReadOnlyProperty { get; }

    [CsvInclude]
    private T? NullablePrivateProperty { get; set; }

    [CsvInclude]
    private T? NullablePrivateReadOnlyProperty { get; }

    public T? NullableField;

    [CsvInclude]
    public T? NullableReadOnlyField;

    [CsvInclude]
    private T? NullablePrivateField;

    [CsvInclude]
    private T? NullablePrivateReadOnlyField;

    public T? GetNullablePrivateProperty => NullablePrivateProperty;

    public T? GetNullablePrivateReadOnlyProperty => NullablePrivateReadOnlyProperty;

    public T? GetNullablePrivateField => NullablePrivateField;

    public T? GetNullablePrivateReadOnlyField => NullablePrivateReadOnlyField;
}
