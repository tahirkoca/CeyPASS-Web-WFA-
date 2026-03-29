using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Repositories
{
    public class UstYetkiliRepositoryCore : IUstYetkiliRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public UstYetkiliRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public string? GetUstYetkili(string personelId)
        {
            const string sql = @"
SELECT TOP 1 UstYetkiliPersonelId AS Value
FROM dbo.UstYetkililer
WHERE PersonelId = {0}";

            return _context.Database
                .SqlQueryRaw<string>(sql, personelId)
                .FirstOrDefault();
        }

        public List<UstYetkili> GetAll()
        {
            const string sql = @"
SELECT PersonelId, UstYetkiliPersonelId, OlusturmaTarihi
FROM dbo.UstYetkililer
ORDER BY PersonelId";

            return _context.Database.SqlQueryRaw<UstYetkili>(sql).ToList();
        }

        public bool EkleVeyaGuncelle(string personelId, string ustYetkiliPersonelId)
        {
            const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.UstYetkililer WHERE PersonelId = {0})
    UPDATE dbo.UstYetkililer SET UstYetkiliPersonelId = {1} WHERE PersonelId = {0}
ELSE
    INSERT INTO dbo.UstYetkililer(PersonelId, UstYetkiliPersonelId) VALUES ({0}, {1})";

            return _context.Database.ExecuteSqlRaw(sql, personelId, ustYetkiliPersonelId) > 0;
        }

        public bool Sil(string personelId)
        {
            const string sql = @"DELETE FROM dbo.UstYetkililer WHERE PersonelId = {0}";
            return _context.Database.ExecuteSqlRaw(sql, personelId) > 0;
        }

        public List<string> GetSubordinates(string ustYetkiliPersonelId)
        {
            const string sql = "SELECT PersonelId AS Value FROM dbo.UstYetkililer WHERE UstYetkiliPersonelId = {0}";
            return _context.Database.SqlQueryRaw<string>(sql, ustYetkiliPersonelId).ToList();
        }

        public bool AnySubordinates(string ustYetkiliPersonelId)
        {
            const string sql = "SELECT TOP 1 PersonelId AS Value FROM dbo.UstYetkililer WHERE UstYetkiliPersonelId = {0}";
            return _context.Database.SqlQueryRaw<string>(sql, ustYetkiliPersonelId).Any();
        }
    }
}

