using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oucc.AotCsv.Attributes;

namespace Oucc.AotCsv.Test.TestTargets
{
    [CsvSerializable]
    internal partial class SampleClass
    {
        [CsvIndex(3)]
        public int TestInt { get; set; }

        [CsvIndex(2)]
        [CsvDateTimeFormat("MMMM dd dddd")]
        public DateTime TestDateTime { get; set; }

        [CsvIndex(1)]
        [CsvName("TestDateTime?")]
        [CsvDateTimeFormat("HH mm ss", System.Globalization.DateTimeStyles.AssumeUniversal)]
        public DateTime? TestDateTimeNullable;

        public SampleClass(int testInt, DateTime testDateTime, DateTime? testDateTimeNullable)
        {
            TestInt = testInt;
            TestDateTime = testDateTime;
            TestDateTimeNullable = testDateTimeNullable;
        }

        public SampleClass() { }
    }
}
