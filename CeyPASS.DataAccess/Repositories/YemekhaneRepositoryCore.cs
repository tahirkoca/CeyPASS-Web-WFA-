using CeyPASS.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    /// <summary>Yemekhane modülü veri erişimi.</summary>
    public class YemekhaneRepositoryCore : IYemekhaneRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public YemekhaneRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        /// <summary>Insert Limit işlemini ekler.</summary>
        public void InsertLimit(string personelId, int gunlukLimit)
        {
            // Listener aktif limiti olmayan kişiyi yemekhane cihazında pasif eder; iki adım arasında limitsiz an kalmamalı
            var sql = @"
UPDATE dbo.YemekhaneGirisLimitler
   SET AktifMi = 0
 WHERE PersonelId = @p0 AND AktifMi = 1;

INSERT INTO dbo.YemekhaneGirisLimitler(PersonelId, GunlukLimit, KayitTarihi, AktifMi)
VALUES (@p0, @p1, GETDATE(), 1);";

            TekTransactionda(() => _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", gunlukLimit)));
        }

        /// <summary>Upsert Limit işlemini yazar veya günceller.</summary>
        public void UpsertLimit(string personelId, int gunlukLimit)
        {
            // Eşzamanlı iki kayıtta çift satır oluşmasın diye satır kilitlenerek güncellenir, yoksa eklenir
            var sql = @"
UPDATE dbo.YemekhaneGirisLimitler WITH (UPDLOCK, HOLDLOCK)
   SET GunlukLimit = @p1,
       KayitTarihi = GETDATE(),
       AktifMi     = 1
 WHERE PersonelId = @p0;

IF @@ROWCOUNT = 0
    INSERT INTO dbo.YemekhaneGirisLimitler(PersonelId, GunlukLimit, KayitTarihi, AktifMi)
    VALUES (@p0, @p1, GETDATE(), 1);";

            TekTransactionda(() => _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", gunlukLimit)));
        }

        /// <summary>Açık transaction varsa ona katılır; yoksa yeni transaction açıp hata olursa geri alır.</summary>
        private void TekTransactionda(System.Action yaz)
        {
            if (_context.Database.CurrentTransaction != null)
            {
                yaz();
                return;
            }

            using (var tx = _context.Database.BeginTransaction())
            {
                try
                {
                    yaz();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        /// <summary>Pasif Et By Personel işlemini gerçekleştirir.</summary>
        public void PasifEtByPersonel(string personelId)
        {
            var sql = @"
UPDATE dbo.YemekhaneGirisLimitler
   SET AktifMi = 0
 WHERE PersonelId = @p0";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId));
        }

        /// <summary>Move Personel Id işlemini gerçekleştirir.</summary>
        public void MovePersonelId(string oldPersonelId, string newPersonelId)
        {
            var sql = @"
UPDATE dbo.YemekhaneGirisLimitler
   SET PersonelId = @p1
 WHERE PersonelId = @p0";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", oldPersonelId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", newPersonelId));
        }

        /// <summary>Son Gunluk Limit sorgularını getirir.</summary>
        public int? GetSonGunlukLimit(string personelId)
        {
            var limit = _context.YemekhaneGirisLimitler
                .Where(y => y.PersonelId == personelId && y.GunlukLimit.HasValue && y.GunlukLimit.Value > 0)
                .OrderByDescending(y => y.Id)
                .Select(y => y.GunlukLimit)
                .FirstOrDefault();

            if (!limit.HasValue || limit.Value <= 0)
                return null;

            return limit.Value;
        }
    }
}
