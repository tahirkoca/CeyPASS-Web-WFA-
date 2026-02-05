using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class CanliIzlemeRepositoryCore : ICanliIzlemeRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public CanliIzlemeRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<LastPassDTO> GetLastPasses(int firmaId, int take)
        {
            var sql = @"
SELECT TOP (@p1)
    KH.PersonelId                           AS PersonelId,
    ISNULL(CASE 
        WHEN K.PersonelId IS NULL THEN PK.KartAdi
        ELSE (K.Ad + ' ' + K.Soyad) 
    END, N'')                               AS AdSoyad,
    K.Fotograf                              AS Foto,
    ISNULL(D.DepartmanAdi, N'')             AS DepartmanAdi,
    ISNULL(P.PozisyonAdi, N'')              AS Unvan,
    KH.Tarih                                AS Zaman,
    CASE 
        WHEN KH.Tip = N'Giriş'     THEN CAST(1 AS bit)
        WHEN KH.Tip = N'Yemekhane' THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END                                     AS GirisMi,
    ISNULL(C.CihazAdi, N'')                 AS TerminalAdi
FROM KisiHareketler KH
LEFT JOIN Kisiler         K  ON KH.PersonelId = K.PersonelId
LEFT JOIN Departmanlar    D  ON K.DepartmanId = D.DepartmanId
LEFT JOIN Cihazlar        C  ON KH.CihazId    = C.CihazId
LEFT JOIN Pozisyonlar     P  ON K.PozisyonId  = P.PozisyonId
LEFT JOIN PuantajsizKartlar PK ON PK.KartId  = KH.PersonelId
WHERE C.FirmaId = @p0 AND C.AnaGirisCikisMi=1
ORDER BY KH.Tarih DESC";

            return _context.Database
                .SqlQueryRaw<LastPassDTO>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", take))
                .ToList();
        }

        public List<LastPassDTO> GetLastPassesYemekhane(int firmaId, int take)
        {
            var sql = @"
SELECT TOP (@p1)
    KH.PersonelId                           AS PersonelId,
    ISNULL(CASE 
        WHEN K.PersonelId IS NULL THEN PK.KartAdi
        ELSE (K.Ad + ' ' + K.Soyad) 
    END, N'')                               AS AdSoyad,
    K.Fotograf                              AS Foto,
    ISNULL(D.DepartmanAdi, N'')             AS DepartmanAdi,
    ISNULL(P.PozisyonAdi, N'')              AS Unvan,
    KH.Tarih                                AS Zaman,
    CAST(1 AS bit)                          AS GirisMi,
    ISNULL(C.CihazAdi, N'')                 AS TerminalAdi
FROM KisiHareketler KH
LEFT JOIN Kisiler         K  ON KH.PersonelId = K.PersonelId
LEFT JOIN Departmanlar    D  ON K.DepartmanId = D.DepartmanId
LEFT JOIN Cihazlar        C  ON KH.CihazId    = C.CihazId
LEFT JOIN Pozisyonlar     P  ON K.PozisyonId  = P.PozisyonId
LEFT JOIN PuantajsizKartlar PK ON PK.KartId  = KH.PersonelId
WHERE C.FirmaId = @p0
  AND KH.Tip = N'Yemekhane'
ORDER BY KH.Tarih DESC";

            return _context.Database
                .SqlQueryRaw<LastPassDTO>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", take))
                .ToList();
        }

        public AuthUserDTO Validate(int firmaId, string user, string password)
        {
            var sql = @"
SELECT TOP(1) 
       c.KullaniciId,
       c.FirmaId,
       ISNULL(f.FirmaAdi, N'') AS FirmaAdi,
       c.KullaniciAdi,
       c.KullaniciAdi AS AdSoyad,
       c.Rol,
       CAST(NULL AS int) AS RolId
FROM dbo.CanliIzlemeHesaplari c WITH (NOLOCK)
LEFT JOIN dbo.Firmalar f WITH (NOLOCK) ON f.FirmaId = c.FirmaId
WHERE c.FirmaId      = @p0
  AND c.KullaniciAdi = @p1
  AND c.SifreHash    = HASHBYTES('SHA2_256', CONVERT(VARBINARY(200), @p2))
  AND c.AktifMi      = 1";

            return _context.Database
                .SqlQueryRaw<AuthUserDTO>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", (user ?? string.Empty).Trim()),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", password ?? string.Empty))
                .AsEnumerable()
                .FirstOrDefault();
        }
    }
}
