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

        public List<PuantajsizKartAtama> GetTodayActive(DateTime now, int firmaId)
        {
            var today = now.Date;

            var query =
                from a in _context.PuantajsizKartAtamalari
                join k in _context.Kisiler
                    on a.KartId equals k.PersonelId
                where k.FirmaId == firmaId
                      && a.Baslangic.Date == today
                      && a.Bitis == null
                orderby a.Baslangic descending
                select new PuantajsizKartAtama
                {
                    AtamaId = a.AtamaId,
                    KartId = a.KartId,
                    MisafirAdSoyad = a.MisafirAdSoyad,
                    TCKimlikNo = a.TCKimlikNo,
                    ZiyaretEdilenKisi = a.ZiyaretEdilenKisi,
                    KartAdi = (k.Ad != null || k.Soyad != null) ? ((k.Ad ?? "") + " " + (k.Soyad ?? "")).Trim() : "",
                    Baslangic = a.Baslangic,
                    Bitis = a.Bitis,
                    Notlar = a.Notlar
                };

            return query.ToList();
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
                Notlar = a.Notlar
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

            return new PuantajsizKartAtama
            {
                AtamaId = e.AtamaId,
                KartId = e.KartId,
                MisafirAdSoyad = e.MisafirAdSoyad,
                TCKimlikNo = e.TCKimlikNo,
                ZiyaretEdilenKisi = e.ZiyaretEdilenKisi,
                Baslangic = e.Baslangic,
                Bitis = e.Bitis,
                Notlar = e.Notlar
            };
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

            return new PuantajsizKartAtama
            {
                AtamaId = e.AtamaId,
                KartId = e.KartId,
                MisafirAdSoyad = e.MisafirAdSoyad,
                TCKimlikNo = e.TCKimlikNo,
                ZiyaretEdilenKisi = e.ZiyaretEdilenKisi,
                Baslangic = e.Baslangic,
                Bitis = e.Bitis,
                Notlar = e.Notlar
            };
        }
    }
}
