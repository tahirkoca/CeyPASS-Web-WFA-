using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class AdminKullaniciRepositoryCore : IAdminKullaniciRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public AdminKullaniciRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<KullaniciAdminRow> GetAll()
        {
            const string sql = @"
SELECT
    CAST(k.KullaniciId AS int) AS KullaniciId,
    k.KullaniciAdi,
    k.RolId,
    r.RolTanimi,
    k.PersonelId
FROM dbo.Kullanicilar k
LEFT JOIN dbo.Roller r ON r.RolId = k.RolId
ORDER BY k.KullaniciAdi";

            return _context.Database.SqlQueryRaw<KullaniciAdminRow>(sql).ToList();
        }

        public bool SetPersonelId(int kullaniciId, int? personelId)
        {
            const string sql = @"
UPDATE dbo.Kullanicilar
SET PersonelId = {1}
WHERE KullaniciId = {0}";

            return _context.Database.ExecuteSqlRaw(sql, kullaniciId, (object?)personelId ?? DBNull.Value) > 0;
        }
    }
}

