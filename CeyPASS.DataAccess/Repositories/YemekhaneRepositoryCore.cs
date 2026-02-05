using CeyPASS.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CeyPASS.DataAccess.Repositories
{
    public class YemekhaneRepositoryCore : IYemekhaneRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public YemekhaneRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public void InsertLimit(string personelId, int gunlukLimit)
        {
            var sql1 = @"
UPDATE dbo.YemekhaneGirisLimitler
   SET AktifMi = 0
 WHERE PersonelId = @p0 AND AktifMi = 1";

            _context.Database.ExecuteSqlRaw(sql1,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId));

            var sql2 = @"
INSERT INTO dbo.YemekhaneGirisLimitler(PersonelId, GunlukLimit, KayitTarihi, AktifMi)
VALUES (@p0, @p1, GETDATE(), 1)";

            _context.Database.ExecuteSqlRaw(sql2,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", gunlukLimit));
        }

        public void UpsertLimit(string personelId, int gunlukLimit)
        {
            var sql = @"
IF EXISTS (SELECT 1 FROM dbo.YemekhaneGirisLimitler WHERE PersonelId = @p0 AND AktifMi = 1)
BEGIN
    UPDATE dbo.YemekhaneGirisLimitler
       SET GunlukLimit = @p1,
           KayitTarihi = GETDATE(),
           AktifMi     = 1
     WHERE PersonelId = @p0 AND AktifMi = 1;
END
ELSE
BEGIN
    INSERT INTO dbo.YemekhaneGirisLimitler(PersonelId, GunlukLimit, KayitTarihi, AktifMi)
    VALUES (@p0, @p1, GETDATE(), 1);
END";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", gunlukLimit));
        }

        public void PasifEtByPersonel(string personelId)
        {
            var sql = @"
UPDATE dbo.YemekhaneGirisLimitler
   SET AktifMi = 0
 WHERE PersonelId = @p0";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId));
        }

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
    }
}
