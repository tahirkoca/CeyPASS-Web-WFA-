using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CeyPASS.Infrastructure.Helpers
{
    public static class InputHelper
    {
        public static bool TryParseYear(string s, out int year)
        {
            year = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;
            return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out year)
                   && year >= 1900 && year <= 2100;
        }
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
