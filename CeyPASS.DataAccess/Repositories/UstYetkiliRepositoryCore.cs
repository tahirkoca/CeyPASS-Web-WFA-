using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Repositories
{
    /// <summary>Üst yetkili erişimi.</summary>
    public class UstYetkiliRepositoryCore : IUstYetkiliRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public UstYetkiliRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        /// <summary>Ust Yetkili sorgularını getirir.</summary>
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

        /// <summary>All sorgularını getirir.</summary>
        public List<UstYetkili> GetAll()
        {
            const string sql = @"
SELECT PersonelId, UstYetkiliPersonelId, OlusturmaTarihi
FROM dbo.UstYetkililer
ORDER BY PersonelId";

            return _context.Database.SqlQueryRaw<UstYetkili>(sql).ToList();
        }

        /// <summary>Ekle Veya Guncelle işlemini ekler.</summary>
        public bool EkleVeyaGuncelle(string personelId, string ustYetkiliPersonelId)
        {
            const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.UstYetkililer WHERE PersonelId = {0})
    UPDATE dbo.UstYetkililer SET UstYetkiliPersonelId = {1} WHERE PersonelId = {0}
ELSE
    INSERT INTO dbo.UstYetkililer(PersonelId, UstYetkiliPersonelId) VALUES ({0}, {1})";

            return _context.Database.ExecuteSqlRaw(sql, personelId, ustYetkiliPersonelId) > 0;
        }

        /// <summary>Sil işlemini siler.</summary>
        public bool Sil(string personelId)
        {
            const string sql = @"DELETE FROM dbo.UstYetkililer WHERE PersonelId = {0}";
            return _context.Database.ExecuteSqlRaw(sql, personelId) > 0;
        }

        /// <summary>Subordinates sorgularını getirir.</summary>
        public List<string> GetSubordinates(string ustYetkiliPersonelId)
        {
            const string sql = "SELECT PersonelId AS Value FROM dbo.UstYetkililer WHERE UstYetkiliPersonelId = {0}";
            return _context.Database.SqlQueryRaw<string>(sql, ustYetkiliPersonelId).ToList();
        }

        /// <summary>Any Subordinates işlemini gerçekleştirir.</summary>
        public bool AnySubordinates(string ustYetkiliPersonelId)
        {
            const string sql = "SELECT TOP 1 PersonelId AS Value FROM dbo.UstYetkililer WHERE UstYetkiliPersonelId = {0}";
            return _context.Database.SqlQueryRaw<string>(sql, ustYetkiliPersonelId).Any();
        }
    }
}

