using System;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>
    /// Rapor stored procedure'lerine gönderilen tarih aralığı parametreleri.
    /// </summary>
    public static class RaporTarihHelper
    {
        public static DateTime ToReportRangeStart(DateTime d) => d.Date;

        /// <summary>Seçilen günün sonu (23:59:59).</summary>
        public static DateTime ToReportRangeEnd(DateTime d) => d.Date.AddDays(1).AddSeconds(-1);
    }
}
