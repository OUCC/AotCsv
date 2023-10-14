using Microsoft.CodeAnalysis;

namespace Oucc.AotCsv.Generator.Comparer;

internal class GeneratorComparer : IEqualityComparer<(GeneratorAttributeSyntaxContext, Compilation)>
{
    internal static readonly GeneratorComparer Instance = new();

    public bool Equals((GeneratorAttributeSyntaxContext, Compilation) x, (GeneratorAttributeSyntaxContext, Compilation) y)
    {
        return x.Item1.Equals(y.Item1);
    }

    public int GetHashCode((GeneratorAttributeSyntaxContext, Compilation) obj)
    {
        return obj.Item1.GetHashCode();
    }
}
