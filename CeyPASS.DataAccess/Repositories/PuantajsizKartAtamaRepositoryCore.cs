using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class PuantajsizKartAtamaRepositoryCore : IPuantajsizKartAtamaRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public PuantajsizKartAtamaRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<PuantajsizKartAtama> GetTodayActive(DateTime now, int firmaId, bool? ziyaretciMi = null, bool? aracKartiMi = null)
        {
            var today = now.Date;

            var query =
                from a in _context.PuantajsizKartAtamalari
                join k in _context.Kisiler
                    on a.KartId equals k.PersonelId
                where k.FirmaId == firmaId
                      && a.Baslangic.Date == today
                      && a.Bitis == null
                select new { a, k };

            if (ziyaretciMi.HasValue)
                query = query.Where(x => x.k.ZiyaretciMi == ziyaretciMi.Value);

            if (aracKartiMi.HasValue)
                query = query.Where(x => x.k.AracKartiMi == aracKartiMi.Value);

            return query
                .OrderByDescending(x => x.a.Baslangic)
                .Select(x => new PuantajsizKartAtama
                {
                    AtamaId = x.a.AtamaId,
                    KartId = x.a.KartId,
                    MisafirAdSoyad = x.a.MisafirAdSoyad,
                    TCKimlikNo = x.a.TCKimlikNo,
                    ZiyaretEdilenKisi = x.a.ZiyaretEdilenKisi,
                    KartAdi = (x.k.Ad != null || x.k.Soyad != null) ? ((x.k.Ad ?? "") + " " + (x.k.Soyad ?? "")).Trim() : "",
                    Baslangic = x.a.Baslangic,
                    Bitis = x.a.Bitis,
                    Notlar = x.a.Notlar,
                    Plaka = x.a.Plaka
                })
                .ToList();
        }

        public bool CardBelongsToFirma(string personelId, int firmaId)
        {
            return _context.Kisiler
                .Any(k => k.PersonelId == personelId && k.FirmaId == firmaId);
        }

        public bool ExistsActiveForCard(string personelId)
        {
            return _context.PuantajsizKartAtamalari
                .Any(a => a.KartId == personelId && a.Bitis == null);
        }

        public int Insert(PuantajsizKartAtama a)
        {
            var entity = new CeyPASS.DataAccess.PuantajsizKartAtamalari
            {
                KartId = a.KartId,
                MisafirAdSoyad = a.MisafirAdSoyad,
                TCKimlikNo = a.TCKimlikNo,
                ZiyaretEdilenKisi = a.ZiyaretEdilenKisi,
                Baslangic = a.Baslangic,
                Bitis = null,
                Notlar = a.Notlar,
                Plaka = a.Plaka
            };

            _context.PuantajsizKartAtamalari.Add(entity);
            _context.SaveChanges();

            return entity.AtamaId;
        }

        public PuantajsizKartAtama GetById(int id)
        {
            var e = _context.PuantajsizKartAtamalari
                .AsNoTracking()
                .FirstOrDefault(x => x.AtamaId == id);

            if (e == null)
                return null;

            return Map(e);
        }

        public void Update(PuantajsizKartAtama a)
        {
            var entity = _context.PuantajsizKartAtamalari
                .FirstOrDefault(x => x.AtamaId == a.AtamaId);

            if (entity == null)
                return;

            entity.MisafirAdSoyad = a.MisafirAdSoyad;
            entity.TCKimlikNo = a.TCKimlikNo;
            entity.ZiyaretEdilenKisi = a.ZiyaretEdilenKisi;
            entity.Baslangic = a.Baslangic;
            entity.Bitis = a.Bitis;
            entity.Notlar = a.Notlar;
            entity.Plaka = a.Plaka;

            _context.SaveChanges();
        }

        public PuantajsizKartAtama GetSonAtamaByTcKimlikNo(string tcKimlikNo)
        {
            if (string.IsNullOrWhiteSpace(tcKimlikNo))
                return null;

            var tc = tcKimlikNo.Trim();

            var e = _context.PuantajsizKartAtamalari
                .AsNoTracking()
                .Where(x => x.TCKimlikNo == tc)
                .OrderByDescending(x => x.Baslangic)
                .FirstOrDefault();

            if (e == null)
                return null;

            return Map(e);
        }

        public List<GecmisZiyaretciItem> GetGecmisZiyaretciler(int firmaId, string adFilter, bool? ziyaretciMi, bool? aracKartiMi)
        {
            var query =
                from a in _context.PuantajsizKartAtamalari.AsNoTracking()
                join k in _context.Kisiler.AsNoTracking()
                    on a.KartId equals k.PersonelId
                where k.FirmaId == firmaId
                select new { a, k };

            if (ziyaretciMi.HasValue)
                query = query.Where(x => x.k.ZiyaretciMi == ziyaretciMi.Value);

            bool aracListe = aracKartiMi == true;
            if (aracListe)
            {
                query = query.Where(x =>
                    x.k.AracKartiMi == true || x.k.ZiyaretciMi == true);

                query = query.Where(x =>
                    (x.a.MisafirAdSoyad != null && x.a.MisafirAdSoyad != "")
                    || (x.a.Plaka != null && x.a.Plaka != ""));
            }
            else
            {
                if (aracKartiMi.HasValue)
                    query = query.Where(x => x.k.AracKartiMi == aracKartiMi.Value);

                query = query.Where(x => x.a.MisafirAdSoyad != null && x.a.MisafirAdSoyad != "");
            }

            if (!string.IsNullOrWhiteSpace(adFilter))
            {
                var filter = adFilter.Trim();
                query = query.Where(x =>
                    (x.a.MisafirAdSoyad != null && x.a.MisafirAdSoyad.Contains(filter))
                    || (x.a.Plaka != null && x.a.Plaka.Contains(filter)));
            }

            var raw = query
                .Select(x => new
                {
                    x.a.MisafirAdSoyad,
                    x.a.TCKimlikNo,
                    x.a.ZiyaretEdilenKisi,
                    x.a.Plaka,
                    x.a.Baslangic
                })
                .ToList();

            return raw
                .GroupBy(x =>
                    (x.MisafirAdSoyad ?? "").Trim() + "|" + (x.Plaka ?? "").Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(x => x.Baslangic).First())
                .OrderByDescending(x => x.Baslangic)
                .Select(x => new GecmisZiyaretciItem
                {
                    AdSoyad = x.MisafirAdSoyad ?? "",
                    TCKimlikNo = x.TCKimlikNo,
                    ZiyaretEdilenKisi = x.ZiyaretEdilenKisi,
                    Plaka = x.Plaka,
                    SonZiyaret = x.Baslangic
                })
                .ToList();
        }

        private static PuantajsizKartAtama Map(PuantajsizKartAtamalari e) => new PuantajsizKartAtama
        {
            AtamaId = e.AtamaId,
            KartId = e.KartId,
            MisafirAdSoyad = e.MisafirAdSoyad,
            TCKimlikNo = e.TCKimlikNo,
            ZiyaretEdilenKisi = e.ZiyaretEdilenKisi,
            Baslangic = e.Baslangic,
            Bitis = e.Bitis,
            Notlar = e.Notlar,
            Plaka = e.Plaka
        };
    }
}
