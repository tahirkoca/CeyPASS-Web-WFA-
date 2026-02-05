using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class IsyeriRepositoryCore : IIsyeriRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public IsyeriRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<LookupItem> GetByFirma(int firmId)
        {
            var sql = @"
SELECT IsyeriId AS Id, IsyeriAdi AS Ad
FROM   Isyerler
WHERE  FirmaId = @p0
ORDER BY IsyeriAdi";

            return _context.Database
                .SqlQueryRaw<LookupItem>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", firmId))
                .AsEnumerable()
                .ToList();
        }

        public List<IsyeriItem> GetIsyerleriByFirma(int firmaId)
        {
            var sql = @"
SELECT
    IsyeriId,
    FirmaId,
    IsyeriAdi
FROM dbo.Isyerler WITH (NOLOCK)
WHERE TaseronMu = 0 AND FirmaId = @p0
ORDER BY IsyeriAdi";

            var rows = _context.Database
                .SqlQueryRaw<IsyeriRow>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId))
                .ToList();

            var list = new List<IsyeriItem>();

            foreach (var r in rows)
            {
                list.Add(new IsyeriItem(r.FirmaId, r.IsyeriId, r.IsyeriAdi));
            }
            return list;
        }

        public DataTable GetAll()
        {
            var sql = @"
SELECT FirmaId, IsyeriId, IsyeriAdi
FROM   dbo.Isyerler
ORDER BY FirmaId, IsyeriId";

            var rows = _context.Database
                .SqlQueryRaw<IsyeriRow>(sql)
                .ToList();

            var dt = new DataTable();
            dt.Columns.Add("FirmaId", typeof(int));
            dt.Columns.Add("IsyeriId", typeof(int));
            dt.Columns.Add("IsyeriAdi", typeof(string));

            foreach (var r in rows)
            {
                dt.Rows.Add(r.FirmaId, r.IsyeriId, r.IsyeriAdi);
            }
            return dt;
        }

        public bool InsertManual(int firmaId, int isyeriId, string isyeriAdi)
        {
            var sql = @"
INSERT INTO dbo.Isyerler (FirmaId, IsyeriId, IsyeriAdi)
VALUES (@p0, @p1, @p2)";

            var affected = _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", isyeriId),
                new Microsoft.Data.SqlClient.SqlParameter("@p2", string.IsNullOrWhiteSpace(isyeriAdi) ? (object)DBNull.Value : isyeriAdi));

            return affected > 0;
        }

        public bool Update(int firmaId, int isyeriId, string isyeriAdi)
        {
            var sql = @"
UPDATE dbo.Isyerler
   SET IsyeriAdi = @p2
 WHERE FirmaId   = @p0
   AND IsyeriId  = @p1";

            var affected = _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", isyeriId),
                new Microsoft.Data.SqlClient.SqlParameter("@p2", string.IsNullOrWhiteSpace(isyeriAdi) ? (object)DBNull.Value : isyeriAdi));

            return affected > 0;
        }

        public bool Delete(int firmaId, int isyeriId)
        {
            var sql = @"
DELETE FROM dbo.Isyerler
WHERE FirmaId  = @p0
  AND IsyeriId = @p1";

            var affected = _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", isyeriId));

            return affected > 0;
        }
    }
}
