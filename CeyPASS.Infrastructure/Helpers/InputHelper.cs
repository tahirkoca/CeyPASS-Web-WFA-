using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>Basit metin girişi ayrıştırma yardımcıları.</summary>
    public static class InputHelper
    {
        /// <summary>Yıl değerini 1900–2100 aralığında doğrular.</summary>
        public static bool TryParseYear(string s, out int year)
        {
            year = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out year)
                   && year >= 1900 && year <= 2100;
        }

        /// <summary>Virgülle ayrılmış tam sayı id listesini küme olarak döner.</summary>
        public static HashSet<int> ParseCsvIds(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return new HashSet<int>();
            return new HashSet<int>(
                csv.Split(',')
                   .Select(s => s.Trim())
                   .Where(s => int.TryParse(s, out _))
                   .Select(int.Parse));
        }
    }
}
