using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class CihazRepositoryCore : ICihazRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public CihazRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<CihazListDTO> GetList(bool sadeceAktif, int? firmaId = null)
        {
            var query =
                from c in _context.Cihazlar
                join f in _context.Firmalar on c.FirmaId equals f.FirmaId
                join t in _context.CihazTipler on c.CihazTipi equals t.TipId into ct
                from t in ct.DefaultIfEmpty()
                where (!sadeceAktif || c.AktifMi)
                      && (!firmaId.HasValue || c.FirmaId == firmaId.Value)
                orderby f.FirmaAdi, c.CihazAdi
                select new CihazListDTO
                {
                    CihazId = c.CihazId,
                    FirmaId = c.FirmaId,
                    IPAdres = c.IPAdres,
                    CihazAdi = c.CihazAdi,
                    Port = c.Port,
                    FirmaAdi = f.FirmaAdi,
                    AktifMi = c.AktifMi,
                    Text = c.IPAdres + " " + c.CihazAdi + " [" + f.FirmaAdi + "]"
                };
            return query.ToList();
        }

        public Cihaz GetById(int id)
        {
            var e = _context.Cihazlar
                .AsNoTracking()
                .FirstOrDefault(x => x.CihazId == id);

            if (e == null)
                return null;

            return new Cihaz
            {
                CihazId = e.CihazId,
                FirmaId = e.FirmaId,
                CihazAdi = e.CihazAdi,
                IPAdres = e.IPAdres,
                Port = e.Port,
                Notlar = e.Notlar,
                CihazTipi = e.CihazTipi,
                AktifMi = e.AktifMi,
                BaglandiMi = e.BaglandiMi
            };
        }

        public int Insert(Cihaz c)
        {
            var entity = new CeyPASS.DataAccess.Cihazlar
            {
                FirmaId = c.FirmaId,
                CihazAdi = c.CihazAdi,
                IPAdres = c.IPAdres,
                Port = c.Port,
                Notlar = c.Notlar,
                CihazTipi = (byte)c.CihazTipi,
                AktifMi = true,
                BaglandiMi = false
            };

            _context.Cihazlar.Add(entity);
            _context.SaveChanges();

            return entity.CihazId;
        }

        public void Update(Cihaz c)
        {
            var entity = _context.Cihazlar
                .FirstOrDefault(x => x.CihazId == c.CihazId);

            if (entity == null)
                return;

            entity.FirmaId = c.FirmaId;
            entity.CihazAdi = c.CihazAdi;
            entity.IPAdres = c.IPAdres;
            entity.Port = c.Port;
            entity.Notlar = c.Notlar;
            entity.CihazTipi = (byte)c.CihazTipi;

            _context.SaveChanges();
        }

        public void SetAktif(int id, bool aktif)
        {
            var entity = _context.Cihazlar
                .FirstOrDefault(x => x.CihazId == id);

            if (entity == null)
                return;

            entity.AktifMi = aktif;
            _context.SaveChanges();
        }

        public List<CihazTip> GetTips()
        {
            return _context.CihazTipler
                .AsNoTracking()
                .OrderBy(t => t.TipId)
                .Select(t => new CihazTip
                {
                    TipId = t.TipId,
                    TipAdi = t.TipAdi
                })
                .ToList();
        }
    }
}
