namespace Oucc.AotCsv.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class CsvIncludeAttribute : Attribute { }
