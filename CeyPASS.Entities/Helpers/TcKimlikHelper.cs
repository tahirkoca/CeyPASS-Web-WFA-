using System;
using System.Linq;

namespace CeyPASS.Entities.Helpers
{
    public static class TcKimlikHelper
    {
        public static bool IsValid(string tc)
        {
            if (string.IsNullOrWhiteSpace(tc))
                return false;
            var t = tc.Trim();
            return t.Length == 11 && t.All(char.IsDigit);
        }

        public static bool LooksMasked(string text)
        {
            return !string.IsNullOrEmpty(text) && text.IndexOf('*') >= 0;
        }

        public static string Mask(string tc)
        {
            if (string.IsNullOrWhiteSpace(tc))
                return "";
            var t = tc.Trim();
            if (t.Length <= 1)
                return t;
            return t[0] + new string('*', t.Length - 1);
        }

        public static string RequireValid(string tc)
        {
            var t = (tc ?? "").Trim();
            if (string.IsNullOrEmpty(t))
                throw new ArgumentException("T.C. Kimlik No giriniz.");
            if (LooksMasked(t) || !IsValid(t))
                throw new ArgumentException("T.C. Kimlik No 11 haneli olmalıdır.");
            return t;
        }

        public static string ResolveForSave(string displayText, string tamTc)
        {
            var shown = (displayText ?? "").Trim();
            if (LooksMasked(shown))
                return RequireValid(tamTc);
            return RequireValid(shown);
        }
    }
}
