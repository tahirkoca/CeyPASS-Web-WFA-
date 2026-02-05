using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace CeyPASS.DataAccess.Repositories
{
    public class SistemLogRepositoryCore : ISistemLogRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public SistemLogRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public void Insert(SistemLog log)
        {
            var sql = @"
INSERT INTO dbo.SistemLoglari
(
    LogZamani,
    KullaniciId,
    KullaniciAdi,
    IslemTuru,
    Kaynak,
    Islem,
    Mesaj,
    IpAdres,
    BilgisayarAdi,
    KorelasyonId,
    DetayJson,
    HataMesaji
)
VALUES
(
    GETDATE(),
    @p0,
    (SELECT TOP(1) KullaniciAdi FROM dbo.Kullanicilar WHERE KullaniciId = @p0),
    @p1,
    @p2,
    @p3,
    @p4,
    @p5,
    @p6,
    @p7,
    @p8,
    @p9
)";

            var pUid = new Microsoft.Data.SqlClient.SqlParameter("@p0", SqlDbType.Int) { Value = log.KullaniciId ?? (object)DBNull.Value };
            var pTur = new Microsoft.Data.SqlClient.SqlParameter("@p1", SqlDbType.TinyInt) { Value = (byte)log.IslemTuru };
            var pKay = new Microsoft.Data.SqlClient.SqlParameter("@p2", SqlDbType.NVarChar, 100) { Value = log.Kaynak ?? (object)DBNull.Value };
            var pIslem = new Microsoft.Data.SqlClient.SqlParameter("@p3", SqlDbType.NVarChar, 100) { Value = log.Islem ?? (object)DBNull.Value };
            var pMesaj = new Microsoft.Data.SqlClient.SqlParameter("@p4", SqlDbType.NVarChar, 2000) { Value = log.Mesaj ?? (object)DBNull.Value };
            var pIp = new Microsoft.Data.SqlClient.SqlParameter("@p5", SqlDbType.NVarChar, 100) { Value = log.IpAdres ?? (object)DBNull.Value };
            var pPc = new Microsoft.Data.SqlClient.SqlParameter("@p6", SqlDbType.NVarChar, 200) { Value = log.BilgisayarAdi ?? (object)DBNull.Value };
            var pCid = new Microsoft.Data.SqlClient.SqlParameter("@p7", SqlDbType.NVarChar, 50) { Value = log.KorelasyonId ?? (object)DBNull.Value };
            var pDetay = new Microsoft.Data.SqlClient.SqlParameter("@p8", SqlDbType.NVarChar) { Value = log.DetayJson ?? (object)DBNull.Value };
            var pHata = new Microsoft.Data.SqlClient.SqlParameter("@p9", SqlDbType.NVarChar) { Value = log.HataMesaji ?? (object)DBNull.Value };

            _context.Database.ExecuteSqlRaw(sql, pUid, pTur, pKay, pIslem, pMesaj, pIp, pPc, pCid, pDetay, pHata);
        }
    }
}
