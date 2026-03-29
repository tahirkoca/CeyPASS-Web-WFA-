using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class IzinTalepRepositoryCore : IIzinTalepRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public IzinTalepRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public int Ekle(IzinTalep talep)
        {
            var outIdParam = new Microsoft.Data.SqlClient.SqlParameter
            {
                ParameterName = "@outId",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };

            const string sql = @"
INSERT INTO dbo.IzinTalepleri
(
    PersonelId, FirmaId, IzinTipId, Baslangic, Bitis, SaatlikIzinMi,
    Aciklama, IzinAdres, TelefonNo, TalepTarihi,
    TalepImzaKullaniciId, TalepImzaTarihi,
    UstYetkiliPersonelId, UstYetkiliOnayDurumu,
    IkOnayDurumu
)
VALUES
(
    {0}, {1}, {2}, {3}, {4}, {5},
    {6}, {7}, {8}, {9},
    {10}, {11},
    {12}, {13},
    {14}
);
SET {15} = CAST(SCOPE_IDENTITY() as int);";

            _context.Database.ExecuteSqlRaw(
                    sql,
                    talep.PersonelId,
                    talep.FirmaId,
                    (object?)talep.IzinTipId,
                    talep.Baslangic,
                    talep.Bitis,
                    talep.SaatlikIzinMi,
                    (object?)talep.Aciklama,
                    (object?)talep.IzinAdres,
                    (object?)talep.TelefonNo,
                    talep.TalepTarihi,
                    (object?)talep.TalepImzaKullaniciId,
                    (object?)talep.TalepImzaTarihi,
                    (object?)talep.UstYetkiliPersonelId,
                    (object?)((byte?)talep.UstYetkiliOnayDurumu),
                    (object?)((byte?)talep.IkOnayDurumu),
                    outIdParam);

            return outIdParam.Value != DBNull.Value ? (int)outIdParam.Value : 0;
        }

        public IzinTalep? GetById(int talepId)
        {
            const string sql = @"SELECT * FROM dbo.IzinTalepleri WHERE TalepId = {0}";
            return _context.Database.SqlQueryRaw<IzinTalep>(sql, talepId).FirstOrDefault();
        }

        public IzinTalep? GetBySonucKisiIzinId(int kisiIzinId)
        {
            const string sql = @"SELECT * FROM dbo.IzinTalepleri WHERE SonucKisiIzinId = {0}";
            return _context.Database.SqlQueryRaw<IzinTalep>(sql, kisiIzinId).FirstOrDefault();
        }

        public List<IzinTalep> GetByPersonel(string personelId)
        {
            const string sql = @"SELECT * FROM dbo.IzinTalepleri WHERE PersonelId = {0} ORDER BY TalepTarihi DESC";
            return _context.Database.SqlQueryRaw<IzinTalep>(sql, personelId).ToList();
        }

        public List<IzinTalep> GetUstYetkiliBekleyenler(string ustYetkiliPersonelId)
        {
            const string sql = @"
SELECT *
FROM dbo.IzinTalepleri
WHERE UstYetkiliPersonelId = {0}
  AND UstYetkiliOnayDurumu = 0
ORDER BY TalepTarihi DESC";

            return _context.Database.SqlQueryRaw<IzinTalep>(sql, ustYetkiliPersonelId).ToList();
        }

        public List<IzinTalep> GetIkBekleyenler()
        {
            const string sql = @"
SELECT *
FROM dbo.IzinTalepleri
WHERE IkOnayDurumu = 0
ORDER BY TalepTarihi DESC";

            return _context.Database.SqlQueryRaw<IzinTalep>(sql).ToList();
        }

        public bool UstYetkiliGuncelle(int talepId, IzinOnayDurumu durum, string? aciklama)
        {
            const string sql = @"
UPDATE dbo.IzinTalepleri
SET UstYetkiliOnayDurumu = {1},
    UstYetkiliOnayTarihi = GETDATE(),
    UstYetkiliAciklama   = {2},
    IkOnayDurumu         = CASE WHEN {1} = 1 THEN 0 ELSE IkOnayDurumu END
WHERE TalepId = {0}";

            return _context.Database.ExecuteSqlRaw(sql, talepId, (byte)durum, (object?)aciklama) > 0;
        }

        public bool IkGuncelle(int talepId, IzinOnayDurumu durum, int ikKullaniciId, string? aciklama)
        {
            const string sql = @"
UPDATE dbo.IzinTalepleri
SET IkOnayDurumu           = {1},
    IkOnaylayanKullaniciId = {2},
    IkOnayTarihi           = GETDATE(),
    IkAciklama             = {3}
WHERE TalepId = {0}";

            return _context.Database.ExecuteSqlRaw(sql, talepId, (byte)durum, ikKullaniciId, (object?)aciklama) > 0;
        }

        public bool SetSonucKisiIzinId(int talepId, int kisiIzinId)
        {
            const string sql = @"UPDATE dbo.IzinTalepleri SET SonucKisiIzinId = {1} WHERE TalepId = {0}";
            return _context.Database.ExecuteSqlRaw(sql, talepId, kisiIzinId) > 0;
        }

        public bool DonusImzasinaAc(int talepId, int ikKullaniciId)
        {
            const string sql = @"
UPDATE dbo.IzinTalepleri
SET KullanimImzaIstenen = 1,
    KullanimImzaIstenmeTarihi = GETDATE(),
    KullanimImzaIstenmeKullaniciId = {1}
WHERE TalepId = {0}";
            return _context.Database.ExecuteSqlRaw(sql, talepId, ikKullaniciId) > 0;
        }

        public bool KullanimImzaAt(int talepId, int personelKullaniciId)
        {
            const string sql = @"
UPDATE dbo.IzinTalepleri
SET KullanimImzaKullaniciId = {1},
    KullanimImzaTarihi = GETDATE()
WHERE TalepId = {0} AND KullanimImzaIstenen = 1 AND KullanimImzaTarihi IS NULL";

            object? pId = personelKullaniciId > 0 ? personelKullaniciId : null;
            return _context.Database.ExecuteSqlRaw(sql, talepId, pId) > 0;
        }
    }
}

