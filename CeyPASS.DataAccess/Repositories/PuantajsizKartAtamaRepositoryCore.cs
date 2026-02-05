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
                join k in _context.PuantajsizKartlar
                    on a.KartId equals k.KartId
                where k.FirmaId == firmaId
                      && a.Baslangic.Date == today
                      && a.Bitis == null
                orderby a.Baslangic descending
                select new PuantajsizKartAtama
                {
                    AtamaId = a.AtamaId,
                    KartId = a.KartId,
                    MisafirAdSoyad = a.MisafirAdSoyad,
                    KartAdi = k.KartAdi,
                    Baslangic = a.Baslangic,
                    Bitis = a.Bitis,
                    Notlar = a.Notlar
                };

            return query.ToList();
        }

        public bool CardBelongsToFirma(int kartId, int firmaId)
        {
            var kartIdStr = kartId.ToString();

            return _context.PuantajsizKartlar
                .Any(k => k.KartId == kartIdStr && k.FirmaId == firmaId);
        }

        public bool ExistsActiveForCard(int kartId)
        {
            var kartIdStr = kartId.ToString();

            return _context.PuantajsizKartAtamalari
                .Any(a => a.KartId == kartIdStr && a.Bitis == null);
        }

        public int Insert(PuantajsizKartAtama a)
        {
            var entity = new CeyPASS.DataAccess.PuantajsizKartAtamalari
            {
                KartId = a.KartId,
                MisafirAdSoyad = a.MisafirAdSoyad,
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
            entity.Baslangic = a.Baslangic;
            entity.Bitis = a.Bitis;
            entity.Notlar = a.Notlar;

            _context.SaveChanges();
        }
    }
}
