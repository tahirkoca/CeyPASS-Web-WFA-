using System;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>
    /// Rapor stored procedure'lerine gönderilen tarih aralığı parametreleri.
    /// </summary>
    public static class RaporTarihHelper
    {
        /// <summary>Aralık başlangıcı: seçilen günün 00:00:00’ı.</summary>
        public static DateTime ToReportRangeStart(DateTime d) => d.Date;

        /// <summary>Seçilen günün sonu (23:59:59).</summary>
        public static DateTime ToReportRangeEnd(DateTime d) => d.Date.AddDays(1).AddSeconds(-1);
    }
}
