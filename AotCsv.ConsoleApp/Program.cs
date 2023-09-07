using System.Globalization;
using System.Text;
using Oucc.AotCsv;
using Oucc.AotCsv.ConsoleApp.GeneratedCodeTarget;
using Oucc.AotCsv.GeneratorHelpers;

Console.WriteLine("Hello, World!");
var sample0 = new SampleModel() { Id = 0, FirstName = ",", LastName = "a", BirthDay = DateTime.Now };
var sample1 = new SampleModel() { Id = 1, FirstName = "\"", LastName = "b", BirthDay = DateTime.Now };
var sample2 = new SampleModel() { Id = 2, FirstName = "\r\n", LastName = "c", BirthDay = DateTime.Now };
var sample3 = new SampleModel() { Id = 3, FirstName = "\",\r\"\n", LastName = "c", BirthDay = DateTime.Now };
var sampleModels = new List<SampleModel>() { sample0, sample1, sample2, sample3 };

using var ms = new MemoryStream();
using var writer = new StreamWriter(ms, Encoding.Unicode);
var config = new CsvSerializeConfig(CultureInfo.InvariantCulture);
var csvWriter = new CsvWriter(writer,config);
csvWriter.WriteHeader<SampleModel>();
csvWriter.WriteRecords(sampleModels);
writer.Flush();

Console.WriteLine(Encoding.Unicode.GetString(ms.ToArray()));
