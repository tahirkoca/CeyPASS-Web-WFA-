using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class PersonelVardiyaYemekYetkiRepositoryCore : IPersonelVardiyaYemekYetkiRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public PersonelVardiyaYemekYetkiRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public bool FirmaHasSaatPenceresiAktif(int firmaId)
        {
            // Explicit kolon: EF model/DB adı sapmalarında sessiz false dönmesin.
            return _context.Cihazlar.AsNoTracking()
                .Any(c => c.FirmaId == firmaId
                          && EF.Property<bool>(c, nameof(Cihazlar.SaatPenceresiAktifMi)));
        }

        public List<PersonelVardiyaYemekYetki> GetByCalismaSekliId(int calismaSekliId)
        {
            var q =
                from y in _context.PersonelVardiyaYemekYetkileri.AsNoTracking()
                join i in _context.Isyerler.AsNoTracking()
                    on y.IsyeriId equals i.IsyeriId into ij
                from i in ij.DefaultIfEmpty()
                join c in _context.Cihazlar.AsNoTracking()
                    on y.CihazId equals c.CihazId into cj
                from c in cj.DefaultIfEmpty()
                where y.CalismaSekliId == calismaSekliId
                orderby i.IsyeriAdi, c.CihazAdi, y.YemekBaslangicSaati
                select new PersonelVardiyaYemekYetki
                {
                    Id = y.Id,
                    CalismaSekliId = y.CalismaSekliId,
                    IsyeriId = y.IsyeriId,
                    IsyeriAdi = i != null ? i.IsyeriAdi : null,
                    CihazId = y.CihazId,
                    CihazAdi = c != null ? c.CihazAdi : null,
                    YemekBaslangicSaati = y.YemekBaslangicSaati,
                    YemekBitisSaati = y.YemekBitisSaati,
                    AktifMi = y.AktifMi
                };

            return q.ToList();
        }

        public bool ExistsForCihaz(int calismaSekliId, int cihazId, int? excludeId = null)
        {
            var q = _context.PersonelVardiyaYemekYetkileri.AsNoTracking()
                .Where(x => x.CalismaSekliId == calismaSekliId && x.CihazId == cihazId);

            if (excludeId.HasValue)
                q = q.Where(x => x.Id != excludeId.Value);

            return q.Any();
        }

        public int Insert(PersonelVardiyaYemekYetki item)
        {
            var entity = new PersonelVardiyaYemekYetkileri
            {
                CalismaSekliId = item.CalismaSekliId,
                IsyeriId = item.IsyeriId,
                CihazId = item.CihazId,
                YemekBaslangicSaati = item.YemekBaslangicSaati,
                YemekBitisSaati = item.YemekBitisSaati,
                AktifMi = item.AktifMi
            };

            _context.PersonelVardiyaYemekYetkileri.Add(entity);
            _context.SaveChanges();
            return entity.Id;
        }

        public bool Update(PersonelVardiyaYemekYetki item)
        {
            var entity = _context.PersonelVardiyaYemekYetkileri
                .FirstOrDefault(x => x.Id == item.Id);

            if (entity == null)
                return false;

            entity.CalismaSekliId = item.CalismaSekliId;
            entity.IsyeriId = item.IsyeriId;
            entity.CihazId = item.CihazId;
            entity.YemekBaslangicSaati = item.YemekBaslangicSaati;
            entity.YemekBitisSaati = item.YemekBitisSaati;
            entity.AktifMi = item.AktifMi;

            return _context.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var entity = _context.PersonelVardiyaYemekYetkileri
                .FirstOrDefault(x => x.Id == id);

            if (entity == null)
                return false;

            _context.PersonelVardiyaYemekYetkileri.Remove(entity);
            return _context.SaveChanges() > 0;
        }
    }
}
