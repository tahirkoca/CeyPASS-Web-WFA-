using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class KullaniciFirmaIsyeriYetkiRepositoryCore : IKullaniciFirmaIsyeriYetkiRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public KullaniciFirmaIsyeriYetkiRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<FirmaIsyeriYetkiDTO> GetYetkiler(int kullaniciId)
        {
            const string sql = @"
SELECT FirmaId, IsyeriId
FROM dbo.KullaniciFirmaIsyeriYetkileri
WHERE KullaniciId = @p0 AND AktifMi = 1
ORDER BY FirmaId, IsyeriId";

            return _context.Database
                .SqlQueryRaw<FirmaIsyeriYetkiDTO>(sql, new Microsoft.Data.SqlClient.SqlParameter("@p0", kullaniciId))
                .ToList();
        }
    }
}
