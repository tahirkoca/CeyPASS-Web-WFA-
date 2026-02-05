using CeyPASS.Infrastructure.Helpers;

namespace CeyPASS.Web.Data
{
    /// <summary>
    /// Web projesi için DatabaseHelperCore - Infrastructure'daki paylaşılan ayarları kullanır.
    /// Veritabanı ayarları Web ve WFA'da aynıdır (Infrastructure.Helpers.DatabaseHelperCore).
    /// </summary>
    public static class DatabaseHelperCore
    {
        /// <summary>
        /// EF Core için SQL Server connection string (CeyPASS.Web uygulama adıyla).
        /// </summary>
        public static string GetSqlConnectionString()
            => CeyPASS.Infrastructure.Helpers.DatabaseHelperCore.GetSqlConnectionString("CeyPASS.Web");
    }
}
