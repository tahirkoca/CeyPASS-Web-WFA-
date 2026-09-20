using System;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Infrastructure.Helpers
{
    /// <summary>İzin kağıdı HTML’inde dijital onay satırı metinleri.</summary>
    public static class IzinKagitHtmlHelper
    {
        /// <summary>
        /// Onay/red/bekleme durumuna göre imza satırı: reddedildi, bekliyor (refNo=0 ise boş), onaylı ad-soyad ve tarih.
        /// </summary>
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
