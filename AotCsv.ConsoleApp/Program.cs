using System.Globalization;
using System.Text;
using Oucc.AotCsv;
using Oucc.AotCsv.ConsoleApp.GeneratedCodeTarget;

Console.WriteLine("Hello, World!");

{
    var sample0 = new SampleModel() { Id = 0, FirstName = ",", LastName = "a", BirthDay = DateTime.Now };
    var sample1 = new SampleModel() { Id = 1, FirstName = "\"", LastName = "b", BirthDay = DateTime.Now };
    var sample2 = new SampleModel() { Id = 2, FirstName = "\r\n", LastName = "c", BirthDay = DateTime.Now };
    var sample3 = new SampleModel() { Id = 3, FirstName = "\",\r\"\n", LastName = "c", BirthDay = DateTime.Now };
    var sampleModels = new List<SampleModel>() { sample0, sample1, sample2, sample3 };

    using var ms = new MemoryStream();
    using var writer = new StreamWriter(ms, new UnicodeEncoding(false, false));
    var config = new CsvSerializeConfig(CultureInfo.InvariantCulture);
    using var csvWriter = new CsvWriter<SampleModel>(writer, config);
    csvWriter.WriteHeader();
    csvWriter.WriteRecords(sampleModels);
    writer.Flush();

    Console.WriteLine(Encoding.Unicode.GetString(ms.ToArray()));
}

{
    var csv = $$""""""
    ID,名,姓,MiddleName,BirthDay,IsStudent
    0,"名前",名字,middle name,2023年10月14日,true
    1,"""Mario"",""Mario""",Luigi,,2023年10月14日,False
    2,"改行{{"\r\n\n"}}
    ",new,line,2023年10月14日,false{{"\n"}}
    """""";

    using var sr = new StringReader(csv);
    using var csvr = new CsvReader(sr, CsvDeserializeConfig.InvariantCulture with { HasHeader = true });

    var list = csvr.GetRecords<global::Oucc.AotCsv.ConsoleApp.SampleModel>();

    foreach (var record in list)
    {
        Console.WriteLine($"{record.Id}\t{record.FirstName}\t{record.LastName}\t{record.FullName}\t{record.BirthDay}");
    }
}
