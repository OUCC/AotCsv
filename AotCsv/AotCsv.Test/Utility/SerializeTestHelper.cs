using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oucc.AotCsv.Test.TestTargets;

namespace Oucc.AotCsv.Test.Utility
{
    internal static class SerializeTestHelper
    {
        internal static readonly DateTime Now = DateTime.Now;
        internal static readonly DateTime UtcNow = DateTime.UtcNow;
        internal static readonly UnicodeEncoding UnicodeNoBOM = new(false, false, true);
        internal static readonly List<FieldOnlyClass> FieldOnlyClassList = new() {
            new FieldOnlyClass(),
            new FieldOnlyClass(true, false, 1, 0, 1, 0, 10.25m, 10m, 130.123, 530.25, 354f, 255.2f, -16, -160, 16, 160, -32, -320, 32, 320, -3264, -32640, 3264, 32640, -64, -640, 64, 640, 'a', 'n', "normal", "not null")
        };
        internal static readonly List<FieldOnlyClass> FieldOnlyClassListForShouldQuote = new() {
            new FieldOnlyClass(@char: '\"', charNullable: '\n', @string: "Test \"\" Quotes\r\n Test \",", stringNullable: "Test \r only")
        };
        internal static readonly List<PropertyOnlyClass> PropertyOnlyClassList = new() {
            new PropertyOnlyClass(),
            new PropertyOnlyClass(true, false, 1, 0, 1, 0, 10.25m, 10m, 130.123, 530.25, 354f, 255.2f, -16, -160, 16, 160, -32, -320, 32, 320, -3264, -32640, 3264, 32640, -64, -640, 64, 640, 'a', 'n', "normal", "not null")
        };
        internal static readonly List<PropertyOnlyClass> PropertyOnlyClassListForShouldQuote = new() {
            new PropertyOnlyClass(@char: '\"', charNullable: '\n', @string: "Test \"\" Quotes\r\n Test \",", stringNullable: "Test \r only")
        };
        internal static readonly List<SampleClass> SampleClass1List = new()
        {
            new SampleClass(0, Now, null),
            new SampleClass(100, Now, UtcNow)
        };
        internal static readonly List<SampleClass> SampleClass2List = new()
        {
            new SampleClass(100, Now, UtcNow)
        };
        internal static readonly string MustNoQuoteExpected = $"False,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,{'\0'},,,{Environment.NewLine}True,False,1,0,1,0,10.25,10,130.123,530.25,354,255.2,-16,-160,16,160,-32,-320,32,320,-3264,-32640,3264,32640,-64,-640,64,640,a,n,normal,not null{Environment.NewLine}";
        internal static readonly string MustQuoteExpected = $"""
            "False","","0","","0","","0","","0","","0","","0","","0","","0","","0","","0","","0","","0","","0","","{'\0'}","","",""{Environment.NewLine}"True","False","1","0","1","0","10.25","10","130.123","530.25","354","255.2","-16","-160","16","160","-32","-320","32","320","-3264","-32640","3264","32640","-64","-640","64","640","a","n","normal","not null"{Environment.NewLine}
            """;
        internal static readonly string ShouldQuoteExpected = $"""""
            False,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,0,,"""","{'\n'}","Test """" Quotes{"\r\n"} Test "",","Test {'\r'} only"{Environment.NewLine}
            """"";
        internal static readonly string MustNoQuoteOrShouldQuoteWithoutAttributesExpected = $"""
            FieldBoolean,FieldBooleanNullable,FieldByte,FieldByteNullable,FieldSByte,FieldSByteNullable,FieldDecimal,FieldDecimalNullable,FieldDouble,FieldDoubleNullable,FieldFloat,FieldFloatNullable,FieldInt16,FieldInt16Nullable,FieldUInt16,FieldUInt16Nullable,FieldInt32,FieldInt32Nullable,FieldUInt32,FieldUInt32Nullable,FieldIntPtr,FieldIntPtrNullable,FieldUIntPtr,FieldUIntPtrNullable,FieldInt64,FieldInt64Nullable,FieldUInt64,FieldUInt64Nullable,FieldChar,FieldCharNullable,FieldString,FieldStringNullable{Environment.NewLine}
            """;
        internal static readonly string MustQuoteWithoutAttributesExpected = $"""
            "FieldBoolean","FieldBooleanNullable","FieldByte","FieldByteNullable","FieldSByte","FieldSByteNullable","FieldDecimal","FieldDecimalNullable","FieldDouble","FieldDoubleNullable","FieldFloat","FieldFloatNullable","FieldInt16","FieldInt16Nullable","FieldUInt16","FieldUInt16Nullable","FieldInt32","FieldInt32Nullable","FieldUInt32","FieldUInt32Nullable","FieldIntPtr","FieldIntPtrNullable","FieldUIntPtr","FieldUIntPtrNullable","FieldInt64","FieldInt64Nullable","FieldUInt64","FieldUInt64Nullable","FieldChar","FieldCharNullable","FieldString","FieldStringNullable"{Environment.NewLine}
            """;
        internal static readonly string ShouldOrMustNoQuoteSampleClassExpected = $"""
            ,{Now.ToString("MMMM dd dddd", CultureInfo.InvariantCulture)},0{Environment.NewLine}{UtcNow.ToString("HH mm ss", CultureInfo.InvariantCulture)},{Now.ToString("MMMM dd dddd", CultureInfo.InvariantCulture)},100{Environment.NewLine}
            """;
        internal static readonly string MustQuoteSampleClassExpected = $"""
            "","{Now.ToString("MMMM dd dddd", CultureInfo.InvariantCulture)}","0"{Environment.NewLine}"{UtcNow.ToString("HH mm ss", CultureInfo.InvariantCulture)}","{Now.ToString("MMMM dd dddd", CultureInfo.InvariantCulture)}","100"{Environment.NewLine}
            """;
        internal static readonly string MustNoQuoteOrShouldQuoteWithAttributesExpected = $"""
            TestDateTime?,TestDateTime,TestInt{Environment.NewLine}
            """;
        internal static readonly string MustQuoteWithAttributesExpected = $"""
            "TestDateTime?","TestDateTime","TestInt"{Environment.NewLine}
            """;
    }
}
