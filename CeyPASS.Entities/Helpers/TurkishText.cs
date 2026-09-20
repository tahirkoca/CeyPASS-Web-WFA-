using System.Globalization;

namespace CeyPASS.Entities.Helpers
{
    /// <summary>Türkçe (tr-TR) kültürüne göre büyük/küçük harf duyarsız metin yardımcıları.</summary>
    public static class TurkishText
    {
        private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

        /// <summary>Metni Türkçe kurallarla küçük harfe çevirir (İ→i, I→ı).</summary>
        public static string ToLower(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return Tr.TextInfo.ToLower(s);
        }

        /// <summary>haystack içinde needle arar; Türkçe IgnoreCase. Boş needle = eşleşme var.</summary>
        public static bool ContainsIgnoreCase(string? haystack, string? needle)
        {
            if (string.IsNullOrEmpty(needle)) return true;
            if (string.IsNullOrEmpty(haystack)) return false;
            return Tr.CompareInfo.IndexOf(haystack, needle, CompareOptions.IgnoreCase) >= 0;
        }
    }
}
