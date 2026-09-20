using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Business.Services
{
    /// <summary>Misafir/Araç atama listesi durum birleştirme.</summary>
    public static class KartAtamaListeBuilder
    {
        /// <summary>Kart, açık atama ve son hareket yönünden liste satırı üretir.</summary>
        public static List<KartAtamaListeItem> Build(
            List<KisiListItem> kartlar,
            List<PuantajsizKartAtama> openAtamalar,
            IReadOnlyDictionary<string, PersonelSonHareketYon> sonHareketByPersonelId)
        {
            var atamaByKart = (openAtamalar ?? new List<PuantajsizKartAtama>())
                .Where(a => !string.IsNullOrWhiteSpace(a.KartId))
                .GroupBy(a => a.KartId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Baslangic).First(), StringComparer.OrdinalIgnoreCase);

            var sonuc = new List<KartAtamaListeItem>();
            foreach (var k in kartlar ?? new List<KisiListItem>())
            {
                if (string.IsNullOrWhiteSpace(k.PersonelId)) continue;
                var pid = k.PersonelId.Trim();
                atamaByKart.TryGetValue(pid, out var atama);
                bool hasAtama = atama != null;

                bool? girisMiAfter = null;
                if (hasAtama
                    && sonHareketByPersonelId != null
                    && sonHareketByPersonelId.TryGetValue(pid, out var yon)
                    && yon != null
                    && yon.Tarih >= atama.Baslangic)
                {
                    girisMiAfter = yon.GirisMi;
                }

                sonuc.Add(new KartAtamaListeItem
                {
                    PersonelId = pid,
                    KartAdi = string.IsNullOrWhiteSpace(k.AdSoyad) ? pid : k.AdSoyad.Trim(),
                    AtamaId = atama?.AtamaId,
                    MisafirAdSoyad = atama?.MisafirAdSoyad,
                    Plaka = atama?.Plaka,
                    Durum = KartAtamaListeDurumHelper.Resolve(hasAtama, girisMiAfter)
                });
            }

            return sonuc
                .OrderBy(x => x.KartAdi, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>Son hareket listesini personelId sözlüğüne indirger.</summary>
        public static Dictionary<string, PersonelSonHareketYon> ToDictionary(List<PersonelSonHareketYon> list)
        {
            return (list ?? new List<PersonelSonHareketYon>())
                .Where(x => !string.IsNullOrWhiteSpace(x.PersonelId))
                .GroupBy(x => x.PersonelId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Tarih).First(), StringComparer.OrdinalIgnoreCase);
        }
    }
}
