using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class PozisyonRepositoryCore : IPozisyonRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public PozisyonRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<LookupItem> GetByFirma()
        {
            return _context.Pozisyonlar
                .AsNoTracking()
                .OrderBy(p => p.PozisyonAdi)
                .Select(p => new LookupItem
                {
                    Id = p.PozisyonId,
                    Ad = p.PozisyonAdi ?? string.Empty
                })
                .ToList();
        }

        public List<LookupItem> GetAll()
        {
            return _context.Pozisyonlar
                .AsNoTracking()
                .OrderBy(p => p.PozisyonAdi)
                .Select(p => new LookupItem
                {
                    Id = p.PozisyonId,
                    Ad = p.PozisyonAdi ?? string.Empty
                })
                .ToList();
        }

        public List<PozisyonListDTO> GetListForAdmin()
        {
            return _context.Pozisyonlar
                .AsNoTracking()
                .OrderBy(p => p.PozisyonAdi)
                .Select(p => new PozisyonListDTO
                {
                    Id = p.PozisyonId,
                    Ad = p.PozisyonAdi ?? string.Empty,
                    Aciklama = p.Aciklama ?? string.Empty
                })
                .ToList();
        }

        public DataRow GetById(int id)
        {
            var entity = _context.Pozisyonlar
                .AsNoTracking()
                .FirstOrDefault(p => p.PozisyonId == id);

            if (entity == null)
                return null;

            var dt = new DataTable();
            dt.Columns.Add("PozisyonId", typeof(int));
            dt.Columns.Add("PozisyonAdi", typeof(string));
            dt.Columns.Add("Aciklama", typeof(string));

            dt.Rows.Add(entity.PozisyonId, entity.PozisyonAdi, entity.Aciklama);
            return dt.Rows[0];
        }

        public bool Insert(string ad, string aciklama)
        {
            var entity = new CeyPASS.DataAccess.Pozisyonlar
            {
                PozisyonAdi = string.IsNullOrWhiteSpace(ad) ? null : ad,
                Aciklama = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama
            };

            _context.Pozisyonlar.Add(entity);
            return _context.SaveChanges() > 0;
        }

        public bool Update(int id, string ad, string aciklama)
        {
            var entity = _context.Pozisyonlar
                .FirstOrDefault(p => p.PozisyonId == id);

            if (entity == null)
                return false;

            entity.PozisyonAdi = string.IsNullOrWhiteSpace(ad) ? null : ad;
            entity.Aciklama = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama;

            return _context.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var entity = _context.Pozisyonlar
                .FirstOrDefault(p => p.PozisyonId == id);

            if (entity == null)
                return false;

            _context.Pozisyonlar.Remove(entity);
            return _context.SaveChanges() > 0;
        }
    }
}
