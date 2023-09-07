using Oucc.AotCsv.Attributes;

namespace Oucc.AotCsv.ConsoleApp.GeneratedCodeTarget;

internal partial class SampleModel
{
    [CsvName("ID")]
    public int Id { get; set; }

    [CsvName("名")]
    public required string FirstName { get; init; }

    public string? MiddleName { private get; set; }

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

    [CsvDateTimeFormat("yyyy年MM月dd日")]
    public DateTime BirthDay { get; set; }

}
