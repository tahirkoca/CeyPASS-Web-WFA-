using System;
using System.Linq;

namespace CeyPASS.Entities.Helpers
{
    /// <summary>T.C. kimlik ve pasaport alanları için doğrulama ve maskeleme.</summary>
    public static class TcKimlikHelper
    {
        /// <summary>11 haneli sayısal format kontrolü (checksum yok).</summary>
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

        /// <summary>İlk hane görünür, kalanı yıldız; listelerde KVKK için.</summary>
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

        /// <summary>
        /// TC veya Pasaport'tan en az biri dolu olmalı.
        /// TC doluysa 11 hane doğrulanır; boşsa kontrol edilmez.
        /// </summary>
        public static (string TcKimlikNo, string PasaportNo) RequireTcOrPasaport(string tcKimlikNo, string pasaportNo)
        {
            var tc = (tcKimlikNo ?? "").Trim();
            var pasaport = (pasaportNo ?? "").Trim();

            if (string.IsNullOrEmpty(tc) && string.IsNullOrEmpty(pasaport))
                throw new ArgumentException("T.C. Kimlik No veya Pasaport No giriniz.");

            if (!string.IsNullOrEmpty(tc))
            {
                if (LooksMasked(tc) || !IsValid(tc))
                    throw new ArgumentException("T.C. Kimlik No 11 haneli olmalıdır.");
            }
            else
            {
                tc = null;
            }

            if (string.IsNullOrEmpty(pasaport))
                pasaport = null;
            else if (pasaport.Length > 50)
                throw new ArgumentException("Pasaport No en fazla 50 karakter olabilir.");

            return (tc, pasaport);
        }

        /// <summary>UI'da maskeli gösterim varsa tam TC gizli alandan okunur.</summary>
        public static string ResolveForSave(string displayText, string tamTc)
        {
            var shown = (displayText ?? "").Trim();
            if (LooksMasked(shown))
                return RequireValid(tamTc);
            return RequireValid(shown);
        }

        /// <summary>
        /// TC boş bırakılabilir (pasaport ile kayıt). Doluysa 11 hane doğrulanır.
        /// </summary>
        public static string ResolveTcOptionalForSave(string displayText, string tamTc)
        {
            var shown = (displayText ?? "").Trim();
            if (string.IsNullOrEmpty(shown))
                return null;
            if (LooksMasked(shown))
            {
                var t = (tamTc ?? "").Trim();
                if (string.IsNullOrEmpty(t) || LooksMasked(t) || !IsValid(t))
                    throw new ArgumentException("T.C. Kimlik No 11 haneli olmalıdır.");
                return t;
            }
            if (!IsValid(shown))
                throw new ArgumentException("T.C. Kimlik No 11 haneli olmalıdır.");
            return shown;
        }
    }
}
