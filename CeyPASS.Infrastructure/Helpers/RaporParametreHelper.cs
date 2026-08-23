using System;
using System.Collections.Generic;

namespace CeyPASS.Infrastructure.Helpers
{
    public static class RaporParametreHelper
    {
        public const string FirmaIdList = "@FirmaIdList";
        public const string IsyeriIdList = "@IsyeriIdList";
        public const string CihazIdList = "@CihazIdList";
        public const string TarihBaslangic = "@TarihBaslangic";
        public const string TarihBitis = "@TarihBitis";

        public enum MultiSelectKind
        {
            None,
            Isyeri,
            Cihaz
        }

        public static string Normalize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";
            var n = name.Trim();
            if (!n.StartsWith("@", StringComparison.Ordinal))
                n = "@" + n;
            return n;
        }

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

        public static MultiSelectKind GetMultiSelect(IEnumerable<string> names)
        {
            if (HasParam(names, CihazIdList))
                return MultiSelectKind.Cihaz;
            if (HasParam(names, IsyeriIdList))
                return MultiSelectKind.Isyeri;
            return MultiSelectKind.None;
        }
    }
}
