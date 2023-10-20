using Oucc.AotCsv.Attributes;

namespace Oucc.AotCsv.ConsoleApp;

[CsvSerializable]
internal partial class SampleModel<T,S> where T : struct, ISpanFormattable, ISpanParsable<T>
{
    [CsvInclude]
    public T Data { get; set; }

    [CsvName("ID")]
    public int Id { get; set; }

    [CsvName("名")]
    public required string FirstName { get; init; }

    [CsvIndex(2)]
    [CsvName("MiddleName")]
    public string? MiddleName { get; private set; }

    [CsvIndex(1)]
    [CsvName("姓")]
    public required string LastName { get; init; }

    [CsvIgnore]
    public string FullName
    {
        get => $"{FirstName} {MiddleName} {LastName}";
        set
        {
            var names = value.Split();
            if (names is [_, var middle, _])
            {
                MiddleName = middle;
            }
        }
    }

    [CsvIndex(2)]
    [CsvDateTimeFormat("yyyy年MM月dd日")]
    public DateTime BirthDay { get; set; }

    [CsvInclude]
    [CsvIndex(1)]
    public int Age
    {
        get => new DateTime((DateTime.Now - BirthDay).Ticks).Year;
    }

    [CsvInclude]
    public bool? IsStudent { get; set; }

}
