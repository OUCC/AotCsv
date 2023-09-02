namespace Oucc.AotCsv.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class CsvIgnoreAttribute : Attribute { }
