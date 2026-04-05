using System;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>
    /// Connection string çözümlemesi — repoda sıfır secret. Gerçek değerler:
    /// appsettings.Local.json, User Secrets veya ortam değişkeni (ConnectionStrings__DefaultConnection).
    /// </summary>
    public static class DatabaseHelperCore
    {
        /// <summary>
        /// ASP.NET Core ortam değişkeni eşlemesi: ConnectionStrings:DefaultConnection.
        /// </summary>
        public static string? TryGetConnectionStringFromEnvironment()
        {
            var a = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")?.Trim();
            if (!string.IsNullOrEmpty(a)) return a;
            return Environment.GetEnvironmentVariable("CEYPASS_DEFAULT_CONNECTION")?.Trim();
        }

        /// <summary>
        /// Şablon appsettings (YOUR_SERVER / YOUR_USER / YOUR_PASSWORD) veya boş string.
        /// LocalDB gibi gerçek geliştirici connection'ları false döner.
        /// </summary>
        public static bool LooksLikePlaceholder(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) return true;
            return connectionString.Contains("YOUR_PASSWORD", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("YOUR_SERVER", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains("YOUR_USER", StringComparison.OrdinalIgnoreCase);
        }
    }
}
