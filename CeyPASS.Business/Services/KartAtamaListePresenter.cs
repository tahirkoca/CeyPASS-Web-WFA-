using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Business.Services
{
    /// <summary>Misafir/araç atama listesini kısıt haritasıyla birleştirir.</summary>
    public static class KartAtamaListePresenter
    {
        /// <summary>Tip filtresine göre misafir/araç atama satırlarını cihaz kısıtı ile yükler.</summary>
        public static List<KartAtamaListeSatir> Load(
            IMisafirKartService misafir,
            IAracKartiService arac,
            ICanliIzlemeKartKomutService komut,
            int firmaId,
            string tip)
        {
            var key = (tip ?? "tumu").Trim().ToLowerInvariant();
            var wantMisafir = key != "arac";
            var wantArac = key != "misafir";

            var raw = new List<(KartAtamaListeItem item, string tipKey)>();
            if (wantMisafir)
            {
                foreach (var x in misafir.GetAtamaListe(firmaId) ?? new List<KartAtamaListeItem>())
                    raw.Add((x, "misafir"));
            }
            if (wantArac)
            {
                foreach (var x in arac.GetAtamaListe(firmaId) ?? new List<KartAtamaListeItem>())
                    raw.Add((x, "arac"));
            }

            var map = komut.GetCihazdaAktifMap(firmaId, raw.Select(r => r.item.PersonelId));
            return raw
                .Select(r =>
                {
                    var pid = r.item.PersonelId ?? "";
                    var aktif = !map.TryGetValue(pid, out var a) || a;
                    return KartAtamaListeSatir.From(r.item, r.tipKey, aktif);
                })
                .OrderBy(x => x.TipLabel, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.KartAdi, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
    }
}
