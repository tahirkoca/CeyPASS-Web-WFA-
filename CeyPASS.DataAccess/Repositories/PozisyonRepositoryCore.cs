using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    /// <summary>Pozisyon erişimi.</summary>
    public class PozisyonRepositoryCore : IPozisyonRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public PozisyonRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        /// <summary>By Firma sorgularını getirir.</summary>
        public List<LookupItem> GetByFirma(int? firmId = null)
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

        /// <summary>All sorgularını getirir.</summary>
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

        /// <summary>List For Admin sorgularını getirir.</summary>
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

        /// <summary>Kimliğe göre kaydı getirir.</summary>
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

        /// <summary>Yeni kayıt ekler.</summary>
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

        /// <summary>Kaydı günceller.</summary>
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

        /// <summary>Kaydı siler.</summary>
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
