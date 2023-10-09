namespace Oucc.AotCsv.Generator.Comparer;

internal class AttributeComparer : IComparer<List<string>>
{
    internal static readonly AttributeComparer Instance = new();

    public int Compare(List<string> x, List<string> y)
    {
        if (x.Count != 0 && y.Count != 0)
        {
            // ここでx,yは1以上の要素があって、片方が多い時は両方の属性を持つからそっちの方が優先
            if (x.Count != y.Count)
            {
                return x.Count.CompareTo(y.Count) * -1;
            }
            else
            {
                if (x.Count == 2)
                {
                    return 0;
                }
                else
                {
                    // Name(4文字) -> Index(5文字) の順
                    return x[0].Length.CompareTo(y[0].Length);
                }
            }
        }
        else if (x.Count != 0)
        {
            return -1;
        }
        else if (y.Count != 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
