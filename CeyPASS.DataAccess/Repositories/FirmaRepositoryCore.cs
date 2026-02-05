using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class FirmaRepositoryCore : IFirmaRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public FirmaRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public DataTable GetFirmalar()
        {
            var sql = @"
SELECT FirmaId, FirmaAdi
FROM   dbo.Firmalar WITH (NOLOCK)
ORDER BY FirmaAdi";

            var list = _context.Database
                .SqlQueryRaw<FirmaLookupRow>(sql)
                .ToList();

            var dt = new DataTable();
            dt.Columns.Add("FirmaId", typeof(int));
            dt.Columns.Add("FirmaAdi", typeof(string));

            foreach (var x in list)
                dt.Rows.Add(x.FirmaId, x.FirmaAdi);

            return dt;
        }

        public List<Firma> GetPuantajFirmalari()
        {
            var sql = @"
SELECT
    FirmaId,
    FirmaAdi,
    ITBirimMail
FROM dbo.Firmalar WITH (NOLOCK)
ORDER BY FirmaAdi";

            return _context.Database
                .SqlQueryRaw<Firma>(sql)
                .ToList();
        }

        public List<LookupItem> GetSingle(int firmId)
        {
            var sql = @"
SELECT FirmaId AS Id,
       FirmaAdi AS Ad
FROM   Firmalar
WHERE  FirmaId = @p0";

            return _context.Database
                .SqlQueryRaw<LookupItem>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", firmId))
                .AsEnumerable()
                .ToList();
        }

        public List<Firma> GetAll()
        {
            return _context.Firmalar
                .OrderBy(f => f.FirmaAdi)
                .Select(f => new Firma
                {
                    FirmaId = f.FirmaId,
                    FirmaAdi = f.FirmaAdi,
                    ITBirimMail = f.ITBirimMail
                })
                .ToList();
        }

        public bool Insert(Firma f)
        {
            var sql = @"
INSERT INTO Firmalar (FirmaId, FirmaAdi, ITBirimMail)
VALUES (@p0, @p1, @p2)";

            var affected = _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", f.FirmaId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", (object)f.FirmaAdi ?? DBNull.Value),
                new Microsoft.Data.SqlClient.SqlParameter("@p2", (object)f.ITBirimMail ?? DBNull.Value));

            return affected > 0;
        }

        public bool Update(Firma f)
        {
            var sql = @"
UPDATE Firmalar
   SET FirmaAdi    = @p1,
       ITBirimMail = @p2
 WHERE FirmaId     = @p0";

            var affected = _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", f.FirmaId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", (object)f.FirmaAdi ?? DBNull.Value),
                new Microsoft.Data.SqlClient.SqlParameter("@p2", (object)f.ITBirimMail ?? DBNull.Value));

            return affected > 0;
        }

        public bool Delete(int id)
        {
            var sql = @"DELETE FROM Firmalar WHERE FirmaId = @p0";

            var affected = _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", id));

            return affected > 0;
        }

        public int? GetMaxId()
        {
            var sql = @"SELECT MAX(FirmaId) FROM Firmalar";

            return _context.Database
                .SqlQueryRaw<int?>(sql)
                .AsEnumerable()
                .SingleOrDefault();
        }
    }
}
