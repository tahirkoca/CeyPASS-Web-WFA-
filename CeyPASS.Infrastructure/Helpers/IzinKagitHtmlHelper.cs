using System;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Infrastructure.Helpers
{
    public static class IzinKagitHtmlHelper
    {
        public static string DijitalImzaText(string label, string? adSoyad, DateTime? tarih, int? refNo, IzinOnayDurumu? durum = null)
        {
            if (durum == IzinOnayDurumu.Reddedildi)
                return $"{label}: Reddedildi ({adSoyad} — {tarih:dd.MM.yyyy HH:mm})";

            if (!tarih.HasValue)
            {
                if (refNo == 0) return ""; // Geçmiş/Legacy kayıtlar için "Bekliyor" yazmasın
                return $"{label}: Bekliyor";
            }

            var who = string.IsNullOrWhiteSpace(adSoyad) ? "-" : adSoyad;
            var rn = refNo.HasValue ? $" (No:{refNo})" : "";
            return $"{label}: {who} — {tarih:dd.MM.yyyy HH:mm}{rn}";
        }
    }
}

