using System;
using System.Globalization;
using Xunit;

namespace AxeptiaExporter.Tests
{
    public class AdHocTests
    {
        [Fact]
        public void VerifyDatoFormats()
        {
            var a = DateTime.Parse("2010-08-20T15:00:00Z", null, System.Globalization.DateTimeStyles.RoundtripKind);
            var b = DateTime.Parse("2010-08-20T15:00:00.000", null, System.Globalization.DateTimeStyles.RoundtripKind);
            var c = DateTime.Parse("2010-08-20T15:15:14.13", null, System.Globalization.DateTimeStyles.RoundtripKind);

            var d = DateTime.ParseExact("2010-08-20T15:00:00.000", "yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
            var e = DateTime.ParseExact("2010-08-20T16:01:15.1", "yyyy-MM-ddTHH:mm:ss.f", CultureInfo.InvariantCulture);
            var f = DateTime.ParseExact("2010-08-20T15:00:00", "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

            DateTime dateValue;


            var validFormats = new string[]
            {
                "yyyy-MM-ddTHH:mm:ss.fff",
                "yyyy-MM-ddTHH:mm:ss.ff",
                "yyyy-MM-ddTHH:mm:ss.f",
                "yyyy-MM-ddTHH:mm:ss",
            };


            var g = DateTime.TryParseExact("2010-08-20T15:00:00.000", validFormats, CultureInfo.InvariantCulture,
                                     DateTimeStyles.None, out dateValue);
            Assert.True(g);
            var h = DateTime.TryParseExact("2010-08-20T15:00:00.00", validFormats, CultureInfo.InvariantCulture,
                         DateTimeStyles.None, out dateValue);
            Assert.True(g);

            var i = DateTime.TryParseExact("2010-08-20T15:00:00.0", validFormats, CultureInfo.InvariantCulture,
                         DateTimeStyles.None, out dateValue);
            Assert.True(g);



            var j = DateTime.TryParseExact("2010-08-20T15:00:00.00", validFormats, CultureInfo.InvariantCulture,
                         DateTimeStyles.None, out dateValue);
            Assert.True(g);


        }
    }
}
