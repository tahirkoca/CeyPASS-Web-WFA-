using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class AvansRepositoryCore : IAvansRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public AvansRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public int Ekle(AvansTalep talep)
        {
            var outIdParam = new Microsoft.Data.SqlClient.SqlParameter
            {
                ParameterName = "@outId",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };

            const string sql = @"
INSERT INTO dbo.AvansTalepleri
(PersonelId, Miktar, Aciklama, TalepTarihi, Durum)
VALUES
({0}, {1}, {2}, {3}, {4});
SET {5} = CAST(SCOPE_IDENTITY() as int);";

            _context.Database.ExecuteSqlRaw(
                    sql,
                    talep.PersonelId,
                    talep.Miktar,
                    (object?)talep.Aciklama,
                    talep.TalepTarihi,
                    (byte)talep.Durum,
                    outIdParam);

            return outIdParam.Value != DBNull.Value ? (int)outIdParam.Value : 0;
        }

        public List<AvansTalep> GetByPersonel(string personelId)
        {
            const string sql = @"SELECT * FROM dbo.AvansTalepleri WHERE PersonelId = {0} ORDER BY TalepTarihi DESC";
            return _context.Database.SqlQueryRaw<AvansTalep>(sql, personelId).ToList();
        }

        public List<AvansTalep> GetAll()
        {
            const string sql = @"SELECT * FROM dbo.AvansTalepleri ORDER BY TalepTarihi DESC";
            return _context.Database.SqlQueryRaw<AvansTalep>(sql).ToList();
        }

        public AvansTalep? GetById(int avansId)
        {
            const string sql = @"SELECT * FROM dbo.AvansTalepleri WHERE AvansId = {0}";
            return _context.Database.SqlQueryRaw<AvansTalep>(sql, avansId).FirstOrDefault();
        }

        public bool GuncelleOnay(int avansId, AvansDurumu durum, int onaylayanId, string? aciklama)
        {
            const string sql = @"
UPDATE dbo.AvansTalepleri
SET Durum = {1},
    OnaylayanId = {2},
    OnayTarihi = GETDATE(),
    OnayAciklama = {3}
WHERE AvansId = {0}";

            return _context.Database.ExecuteSqlRaw(sql, avansId, (byte)durum, onaylayanId, (object?)aciklama) > 0;
        }

        public bool Sil(int avansId)
        {
            const string sql = @"DELETE FROM dbo.AvansTalepleri WHERE AvansId = {0} AND Durum = 0"; // Sadece bekleyenler silinebilir
            return _context.Database.ExecuteSqlRaw(sql, avansId) > 0;
        }

        public bool Guncelle(int avansId, decimal miktar, string? aciklama)
        {
            const string sql = @"UPDATE dbo.AvansTalepleri SET Miktar = {1}, Aciklama = {2} WHERE AvansId = {0} AND Durum = 0";
            return _context.Database.ExecuteSqlRaw(sql, avansId, miktar, (object?)aciklama) > 0;
        }
    }
}

