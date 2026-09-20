using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.Entities.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    /// <summary>Puantajsiz kart atama erişimi.</summary>
    public class PuantajsizKartAtamaRepositoryCore : IPuantajsizKartAtamaRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public PuantajsizKartAtamaRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        /// <summary>Today Active sorgularını getirir.</summary>
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
                    PasaportNo = x.a.PasaportNo,
                    ZiyaretEdilenKisi = x.a.ZiyaretEdilenKisi,
                    KartAdi = (x.k.Ad != null || x.k.Soyad != null) ? ((x.k.Ad ?? "") + " " + (x.k.Soyad ?? "")).Trim() : "",
                    Baslangic = x.a.Baslangic,
                    Bitis = x.a.Bitis,
                    Notlar = x.a.Notlar,
                    Plaka = x.a.Plaka
                })
                .ToList();
        }

        /// <summary>Open Active sorgularını getirir.</summary>
        public List<PuantajsizKartAtama> GetOpenActive(int firmaId, bool? ziyaretciMi = null, bool? aracKartiMi = null)
        {
            var query =
                from a in _context.PuantajsizKartAtamalari
                join k in _context.Kisiler
                    on a.KartId equals k.PersonelId
                where k.FirmaId == firmaId
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
                    PasaportNo = x.a.PasaportNo,
                    ZiyaretEdilenKisi = x.a.ZiyaretEdilenKisi,
                    KartAdi = (x.k.Ad != null || x.k.Soyad != null) ? ((x.k.Ad ?? "") + " " + (x.k.Soyad ?? "")).Trim() : "",
                    Baslangic = x.a.Baslangic,
                    Bitis = x.a.Bitis,
                    Notlar = x.a.Notlar,
                    Plaka = x.a.Plaka
                })
                .ToList();
        }

        /// <summary>Card Belongs To Firma işlemini gerçekleştirir.</summary>
        public bool CardBelongsToFirma(string personelId, int firmaId)
        {
            return _context.Kisiler
                .Any(k => k.PersonelId == personelId && k.FirmaId == firmaId);
        }

        /// <summary>Exists Active For Card işlemini gerçekleştirir.</summary>
        public bool ExistsActiveForCard(string personelId)
        {
            return _context.PuantajsizKartAtamalari
                .Any(a => a.KartId == personelId && a.Bitis == null);
        }

        /// <summary>Yeni kayıt ekler.</summary>
        public int Insert(PuantajsizKartAtama a)
        {
            var entity = new CeyPASS.DataAccess.PuantajsizKartAtamalari
            {
                KartId = a.KartId,
                MisafirAdSoyad = a.MisafirAdSoyad,
                TCKimlikNo = a.TCKimlikNo,
                PasaportNo = a.PasaportNo,
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

        /// <summary>Kimliğe göre kaydı getirir.</summary>
        public PuantajsizKartAtama GetById(int id)
        {
            var e = _context.PuantajsizKartAtamalari
                .AsNoTracking()
                .FirstOrDefault(x => x.AtamaId == id);

            if (e == null)
                return null;

            return Map(e);
        }

        /// <summary>Kaydı günceller.</summary>
        public void Update(PuantajsizKartAtama a)
        {
            var entity = _context.PuantajsizKartAtamalari
                .FirstOrDefault(x => x.AtamaId == a.AtamaId);

            if (entity == null)
                return;

            entity.MisafirAdSoyad = a.MisafirAdSoyad;
            entity.TCKimlikNo = a.TCKimlikNo;
            entity.PasaportNo = a.PasaportNo;
            entity.ZiyaretEdilenKisi = a.ZiyaretEdilenKisi;
            entity.Baslangic = a.Baslangic;
            entity.Bitis = a.Bitis;
            entity.Notlar = a.Notlar;
            entity.Plaka = a.Plaka;

            _context.SaveChanges();
        }

        /// <summary>Son Atama By Tc Kimlik No sorgularını getirir.</summary>
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

        /// <summary>Geçmiş ziyaretçi/araç listesi; ad/plaka filtresi bellek tarafında tr-TR.</summary>
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

            var raw = query
                .Select(x => new
                {
                    x.a.MisafirAdSoyad,
                    x.a.TCKimlikNo,
                    x.a.PasaportNo,
                    x.a.ZiyaretEdilenKisi,
                    x.a.Plaka,
                    x.a.Notlar,
                    x.a.Baslangic
                })
                .ToList();

            // Ad/plaka filtresi bellek tarafında tr-TR (SQL Contains collation'a bağlı kalmasın)
            if (!string.IsNullOrWhiteSpace(adFilter))
            {
                var filter = adFilter.Trim();
                raw = raw
                    .Where(x =>
                        TurkishText.ContainsIgnoreCase(x.MisafirAdSoyad, filter)
                        || TurkishText.ContainsIgnoreCase(x.Plaka, filter))
                    .ToList();
            }

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
                    PasaportNo = x.PasaportNo,
                    ZiyaretEdilenKisi = x.ZiyaretEdilenKisi,
                    Plaka = x.Plaka,
                    Notlar = x.Notlar,
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
            PasaportNo = e.PasaportNo,
            ZiyaretEdilenKisi = e.ZiyaretEdilenKisi,
            Baslangic = e.Baslangic,
            Bitis = e.Bitis,
            Notlar = e.Notlar,
            Plaka = e.Plaka
        };
    }
}
