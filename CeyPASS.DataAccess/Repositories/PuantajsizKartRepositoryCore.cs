using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class PuantajsizKartRepositoryCore : IPuantajsizKartRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public PuantajsizKartRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public PuantajsizKart GetByKartId(string kartId)
        {
            if (string.IsNullOrWhiteSpace(kartId)) return null;
            var x = _context.PuantajsizKartlar.FirstOrDefault(p => p.KartId == kartId && p.AktifMi);
            if (x == null) return null;
            return new PuantajsizKart
            {
                KartId = x.KartId,
                KartAdi = x.KartAdi,
                KartNo = x.KartNo,
                FirmaId = x.FirmaId,
                CalismaSekli = x.CalismaSekli,
                ZiyaretciMi = x.ZiyaretciMi,
                AracKartiMi = x.AracKartiMi,
                TaseronCalisanMi = x.TaseronCalisanMi,
                AktifMi = x.AktifMi
            };
        }

        public void UpdateByKartId(string kartId, string kartAdi, string kartNo, string calismaSekli)
        {
            if (string.IsNullOrWhiteSpace(kartId)) return;
            var x = _context.PuantajsizKartlar.FirstOrDefault(p => p.KartId == kartId && p.AktifMi);
            if (x == null) return;

            string yeniKartNo = string.IsNullOrWhiteSpace(kartNo) ? null : kartNo.Trim();
            string mevcutKartNo = x.KartNo?.Trim();
            bool kartNoDegisti = (mevcutKartNo ?? "") != (yeniKartNo ?? "");

            if (!kartNoDegisti)
            {
                x.KartAdi = kartAdi ?? x.KartAdi ?? "";
                x.CalismaSekli = string.IsNullOrWhiteSpace(calismaSekli) ? null : calismaSekli;
                _context.SaveChanges();
                return;
            }

            if (!string.IsNullOrWhiteSpace(yeniKartNo) &&
                _context.PuantajsizKartlar.Any(p => p.AktifMi && p.KartNo == yeniKartNo))
                throw new System.InvalidOperationException("Bu kart numarası zaten kayıtlı.");

            PasifEtByKartId(kartId);
            Insert(kartId, yeniKartNo ?? "", kartAdi ?? x.KartAdi ?? "", x.FirmaId, calismaSekli ?? x.CalismaSekli,
                ziyaretciMi: x.ZiyaretciMi ?? false, aracKartMi: x.AracKartiMi ?? false, taseronCalisanMi: x.TaseronCalisanMi ?? false);
        }

        public List<PuantajsizKart> GetByFirmaOrderByName(int firmaId)
        {
            return _context.PuantajsizKartlar
                .Where(x => x.FirmaId == firmaId && x.AktifMi)
                .OrderBy(x => x.KartAdi)
                .Select(x => new PuantajsizKart
                {
                    KartId = x.KartId,
                    KartAdi = x.KartAdi,
                    KartNo = x.KartNo
                })
                .ToList();
        }

        public void Insert(string kartId, string kartNo, string kartAdi, int firmaId, string calismaSekliCsv, bool ziyaretciMi = true, bool aracKartMi = false, bool taseronCalisanMi = false)
        {
            var entity = new CeyPASS.DataAccess.PuantajsizKartlar
            {
                KartId = kartId,
                KartNo = string.IsNullOrWhiteSpace(kartNo) ? null : kartNo,
                KartAdi = kartAdi ?? string.Empty,
                FirmaId = firmaId,
                AktifMi = true,
                CalismaSekli = string.IsNullOrWhiteSpace(calismaSekliCsv) ? null : calismaSekliCsv,
                ZiyaretciMi = ziyaretciMi,
                AracKartiMi = aracKartMi,
                TaseronCalisanMi = taseronCalisanMi
            };

            _context.PuantajsizKartlar.Add(entity);
            _context.SaveChanges();
        }

        public bool Exists(string kartId)
        {
            return _context.PuantajsizKartlar.Any(x => x.KartId == kartId);
        }

        public void UpsertByKartNo(string kartNo, int firmaId, string kartAdi, string calismaSekliCsv)
        {
            var entity = _context.PuantajsizKartlar
                .SingleOrDefault(x => x.KartNo == kartNo);

            if (entity == null)
            {
                entity = new CeyPASS.DataAccess.PuantajsizKartlar
                {
                    KartNo = kartNo,
                    KartAdi = kartAdi,
                    FirmaId = firmaId,
                    AktifMi = true,
                    CalismaSekli = string.IsNullOrWhiteSpace(calismaSekliCsv) ? null : calismaSekliCsv,
                    ZiyaretciMi = false,
                    AracKartiMi = false,
                    TaseronCalisanMi = false
                };

                _context.PuantajsizKartlar.Add(entity);
            }
            else
            {
                entity.KartAdi = kartAdi;
                entity.FirmaId = firmaId;
                entity.AktifMi = true;
                entity.CalismaSekli = string.IsNullOrWhiteSpace(calismaSekliCsv) ? null : calismaSekliCsv;
            }

            _context.SaveChanges();
        }

        public void PasifEtByKartNo(string kartNo)
        {
            var entities = _context.PuantajsizKartlar
                .Where(x => x.KartNo == kartNo)
                .ToList();

            if (!entities.Any())
                return;

            foreach (var e in entities)
                e.AktifMi = false;

            _context.SaveChanges();
        }

        public void PasifEtByKartId(string kartId)
        {
            var entities = _context.PuantajsizKartlar
                .Where(x => x.KartId == kartId)
                .ToList();

            if (!entities.Any())
                return;

            foreach (var e in entities)
                e.AktifMi = false;

            _context.SaveChanges();
        }

        public List<KartItem> GetAktifKartlarForSync()
        {
            return _context.PuantajsizKartlar
                .Where(x => x.AktifMi)
                .OrderBy(x => x.KartAdi)
                .Select(x => new KartItem
                {
                    KartId = x.KartId,
                    KartAdi = x.KartAdi ?? string.Empty,
                    KartNo = x.KartNo ?? string.Empty
                })
                .ToList();
        }

        public List<int> GetAktifKartIdler()
        {
            var ids = _context.PuantajsizKartlar
                .Where(x => x.AktifMi)
                .Select(x => x.KartId)
                .ToList();

            var list = new List<int>();

            foreach (var s in ids)
            {
                if (int.TryParse(s, out var val))
                    list.Add(val);
            }

            return list;
        }

        public void MoveKartId(string oldKartId, string newKartId)
        {
            bool exists = _context.PuantajsizKartlar.Any(x => x.KartId == newKartId);
            if (exists) return;

            var sql = @"
UPDATE dbo.PuantajsizKartlar
   SET KartId = @p1
 WHERE KartId = @p0";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", oldKartId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", newKartId));
        }
    }
}
