namespace Oucc.AotCsv.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class CsvIndexAttribute : Attribute
{
    public uint Index { get; }

    public CsvIndexAttribute(uint index)
    {
        Index = index;
    }
}
