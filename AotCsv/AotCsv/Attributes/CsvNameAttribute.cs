namespace Oucc.AotCsv.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class CsvNameAttribute : Attribute
{
    public string Name { get; }

    public CsvNameAttribute(string name)
    {
        Name = name;
    }
}
