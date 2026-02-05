using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class CalismaSekliRepositoryCore : ICalismaSekliRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public CalismaSekliRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        private static TimeSpan ReadTime(DateTime? dt)
        {
            if (!dt.HasValue)
                return TimeSpan.Zero;

            return dt.Value.TimeOfDay;
        }

        private static DateTime ToDateTime(TimeSpan t)
        {
            return new DateTime(1900, 1, 1).Add(t);
        }

        public List<CalismaSekli> GetAll(int firmaId, bool includeGlobal = true)
        {
            var query = _context.CalismaSekilleri.AsQueryable();

            if (includeGlobal)
                query = query.Where(x => x.FirmaId == firmaId || x.FirmaId == null);
            else
                query = query.Where(x => x.FirmaId == firmaId);

            var list = query.OrderBy(x => x.CalismaSekliAdi).ToList();

            return list.Select(x => new CalismaSekli
            {
                Id = x.CalismaSekilId,
                FirmaId = x.FirmaId ?? 0,
                Ad = x.CalismaSekliAdi,
                Baslangic = ReadTime(x.BaslangicZaman),
                Bitis = ReadTime(x.BitisZaman),
                BaslangicTolerans = ReadTime(x.BaslangicToleransZaman),
                BitisTolerans = ReadTime(x.BitisToleransZaman),
                YemekAktiflestirme = ReadTime(x.YemekAktiflestirmeZaman)
            }).ToList();
        }

        public List<CalismaSekli> GetAllForAdmin()
        {
            var list = _context.CalismaSekilleri
                .OrderBy(x => x.FirmaId)
                .ThenBy(x => x.CalismaSekliAdi)
                .ToList();

            return list.Select(x => new CalismaSekli
            {
                Id = x.CalismaSekilId,
                FirmaId = x.FirmaId ?? 0,
                Ad = x.CalismaSekliAdi,
                Baslangic = ReadTime(x.BaslangicZaman),
                Bitis = ReadTime(x.BitisZaman),
                BaslangicTolerans = ReadTime(x.BaslangicToleransZaman),
                BitisTolerans = ReadTime(x.BitisToleransZaman),
                YemekAktiflestirme = ReadTime(x.YemekAktiflestirmeZaman)
            }).ToList();
        }

        public int Insert(CalismaSekli x)
        {
            var entity = new CeyPASS.DataAccess.CalismaSekilleri
            {
                FirmaId = x.FirmaId,
                CalismaSekliAdi = x.Ad,
                BaslangicZaman = ToDateTime(x.Baslangic),
                BitisZaman = ToDateTime(x.Bitis),
                BaslangicToleransZaman = ToDateTime(x.BaslangicTolerans),
                BitisToleransZaman = ToDateTime(x.BitisTolerans),
                YemekAktiflestirmeZaman = ToDateTime(x.YemekAktiflestirme)
            };

            _context.CalismaSekilleri.Add(entity);
            _context.SaveChanges();
            return entity.CalismaSekilId;
        }

        public bool Update(CalismaSekli x)
        {
            var entity = _context.CalismaSekilleri
                .FirstOrDefault(e => e.CalismaSekilId == x.Id && e.FirmaId == x.FirmaId);

            if (entity == null)
                return false;

            entity.CalismaSekliAdi = x.Ad;
            entity.BaslangicZaman = ToDateTime(x.Baslangic);
            entity.BitisZaman = ToDateTime(x.Bitis);
            entity.BaslangicToleransZaman = ToDateTime(x.BaslangicTolerans);
            entity.BitisToleransZaman = ToDateTime(x.BitisTolerans);
            entity.YemekAktiflestirmeZaman = ToDateTime(x.YemekAktiflestirme);

            return _context.SaveChanges() > 0;
        }

        public bool Delete(int id, int firmaId)
        {
            var entity = _context.CalismaSekilleri
                .FirstOrDefault(e => e.CalismaSekilId == id && e.FirmaId == firmaId);

            if (entity == null)
                return false;

            _context.CalismaSekilleri.Remove(entity);
            return _context.SaveChanges() > 0;
        }
    }
}
