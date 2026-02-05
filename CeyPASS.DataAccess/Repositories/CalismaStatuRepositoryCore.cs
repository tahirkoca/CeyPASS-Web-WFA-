using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class CalismaStatuRepositoryCore : ICalismaStatuRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public CalismaStatuRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<LookupItem> GetByFirma()
        {
            return _context.CalismaStatusu
                .OrderBy(x => x.CalismaStatuAdi)
                .Select(x => new LookupItem
                {
                    Id = x.CalismaStatuId,
                    Ad = x.CalismaStatuAdi
                })
                .ToList();
        }

        public int GetNextId()
        {
            var sql = @"
DECLARE @n int;
SELECT @n = ISNULL(MAX(CalismaStatuId),0) + 1
FROM CalismaStatusu WITH (UPDLOCK, HOLDLOCK);
SELECT @n";

            return _context.Database
                .SqlQueryRaw<int>(sql)
                .AsEnumerable()
                .Single();
        }

        public bool Insert(int id, string ad)
        {
            var sql = @"
INSERT INTO dbo.CalismaStatusu (CalismaStatuId, CalismaStatuAdi)
VALUES (@p0, @p1)";

            var affected = _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", id) { SqlDbType = SqlDbType.Int },
                new Microsoft.Data.SqlClient.SqlParameter("@p1", ad ?? "") { SqlDbType = SqlDbType.NVarChar, Size = 200 });

            return affected > 0;
        }

        public bool Update(int id, string ad)
        {
            var sql = @"
UPDATE dbo.CalismaStatusu
SET CalismaStatuAdi = @p1
WHERE CalismaStatuId = @p0";

            var affected = _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", id) { SqlDbType = SqlDbType.Int },
                new Microsoft.Data.SqlClient.SqlParameter("@p1", ad ?? "") { SqlDbType = SqlDbType.NVarChar, Size = 200 });

            return affected > 0;
        }

        public bool Delete(int id)
        {
            var sql = @"DELETE FROM dbo.CalismaStatusu WHERE CalismaStatuId = @p0";

            var affected = _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", id) { SqlDbType = SqlDbType.Int });

            return affected > 0;
        }
    }
}
