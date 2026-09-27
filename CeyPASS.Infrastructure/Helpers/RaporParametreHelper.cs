using System;
using System.Collections.Generic;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>Rapor stored procedure parametre adları ve çoklu seçim tespiti.</summary>
    public static class RaporParametreHelper
    {
        /// <summary>SP parametresi: firma id listesi.</summary>
        public const string FirmaIdList = "@FirmaIdList";
        /// <summary>SP parametresi: işyeri id listesi.</summary>
        public const string IsyeriIdList = "@IsyeriIdList";
        /// <summary>SP parametresi: cihaz id listesi.</summary>
        public const string CihazIdList = "@CihazIdList";
        /// <summary>SP parametresi: dönem başlangıç tarihi.</summary>
        public const string TarihBaslangic = "@TarihBaslangic";
        /// <summary>SP parametresi: dönem bitiş tarihi.</summary>
        public const string TarihBitis = "@TarihBitis";

        /// <summary>Rapor UI’da hangi çoklu seçim kontrolünün kullanılacağı.</summary>
        public enum MultiSelectKind
        {
            None,
            Isyeri,
            Cihaz
        }

        /// <summary>Parametre adını @ ile başlayacak şekilde standartlaştırır.</summary>
        public static string Normalize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";
            var n = name.Trim();
            if (!n.StartsWith("@", StringComparison.Ordinal))
                n = "@" + n;
            return n;
        }

        /// <summary>Parametre listesinde verilen ad var mı (büyük/küçük harf duyarsız).</summary>
        public static bool HasParam(IEnumerable<string> names, string param)
        {
            if (names == null)
                return false;
            var want = Normalize(param);
            foreach (var n in names)
            {
                if (string.Equals(Normalize(n), want, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>SP parametre setine göre işyeri veya cihaz çoklu seçim modunu döner.</summary>
        public static MultiSelectKind GetMultiSelect(IEnumerable<string> names)
        {
            if (HasParam(names, CihazIdList))
                return MultiSelectKind.Cihaz;
            if (HasParam(names, IsyeriIdList))
                return MultiSelectKind.Isyeri;
            return MultiSelectKind.None;
        }

        /// <summary>Yemekhane cihaz bazlı raporlarda cihaz listesi yalnızca yemekhane tipiyle sınırlanır.</summary>
        public static bool RequiresYemekhaneCihazFilter(string procedureAdi)
        {
            return !string.IsNullOrWhiteSpace(procedureAdi)
                   && procedureAdi.IndexOf("Yemekhane", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
