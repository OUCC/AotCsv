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
        internal static readonly UnicodeEncoding UnicodeNoBOM = new(false, false, true);
    }
}
