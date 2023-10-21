using Oucc.AotCsv.Attributes;

namespace Oucc.AotCsv.Test.TestTargets;

[CsvSerializable]
internal partial class GenericsClass<T> where T : struct, ISpanFormattable, ISpanParsable<T>
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
}
