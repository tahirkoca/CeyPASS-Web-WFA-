using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CeyPASS.DataAccess.Repositories
{
    public class KisiHareketRepositoryCore : IKisiHareketRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public KisiHareketRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public List<KisiHareketDTO> GetLastMovesByFirma(int top, int firmaId)
        {
            var sql = @"
SELECT TOP (@p0)
    KH.Tarih       AS Tarih,
    ISNULL(CASE 
        WHEN K.PersonelId IS NULL THEN PK.KartAdi
        ELSE (K.Ad + ' ' + K.Soyad) 
    END, N'')      AS AdSoyad,
    ISNULL(D.DepartmanAdi, N'') AS Departman,
    ISNULL(P.PozisyonAdi, N'')  AS Unvan,
    ISNULL(C.CihazAdi, N'')     AS CihazAdi,
    KH.PersonelId  AS PersonelId
FROM KisiHareketler KH
LEFT JOIN Kisiler         K  ON KH.PersonelId = K.PersonelId
LEFT JOIN Departmanlar    D  ON K.DepartmanId = D.DepartmanId
LEFT JOIN Cihazlar        C  ON KH.CihazId    = C.CihazId
LEFT JOIN Pozisyonlar     P  ON K.PozisyonId  = P.PozisyonId
LEFT JOIN PuantajsizKartlar PK ON KH.PersonelId = PK.KartId
WHERE C.FirmaId = @p1 AND C.AnaGirisCikisMi=1
ORDER BY KH.Tarih DESC";

            return _context.Database
                .SqlQueryRaw<KisiHareketDTO>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", top),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", firmaId))
                .ToList();
        }

        public List<KisiHareketDTO> GetLastMovesByFirmaYemekhane(int top, int firmaId)
        {
            var sql = @"
SELECT TOP (@p0)
    KH.Tarih       AS Tarih,
    ISNULL(CASE 
        WHEN K.PersonelId IS NULL THEN PK.KartAdi
        ELSE (K.Ad + ' ' + K.Soyad) 
    END, N'')      AS AdSoyad,
    ISNULL(D.DepartmanAdi, N'') AS Departman,
    ISNULL(P.PozisyonAdi, N'')  AS Unvan,
    ISNULL(C.CihazAdi, N'')     AS CihazAdi,
    KH.PersonelId  AS PersonelId
FROM KisiHareketler KH
LEFT JOIN Kisiler         K  ON KH.PersonelId = K.PersonelId
LEFT JOIN Departmanlar    D  ON K.DepartmanId = D.DepartmanId
LEFT JOIN Cihazlar        C  ON KH.CihazId    = C.CihazId
LEFT JOIN Pozisyonlar     P  ON K.PozisyonId  = P.PozisyonId
LEFT JOIN PuantajsizKartlar PK ON KH.PersonelId = PK.KartId
WHERE C.FirmaId = @p1
  AND KH.Tip = N'Yemekhane'
ORDER BY KH.Tarih DESC";

            return _context.Database
                .SqlQueryRaw<KisiHareketDTO>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", top),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", firmaId))
                .ToList();
        }

        public DataTable GetByPersons(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId)
        {
            var sb = new StringBuilder(@"
SELECT
    k.Id,
    f.FirmaAdi AS Firma,                            
    p.PersonelId AS SicilNo,
    p.Ad + ' ' + p.Soyad AS AdSoyad,
    CASE
        WHEN k.CihazId = 0
          OR c.CihazAdi IS NULL
          OR LTRIM(RTRIM(c.CihazAdi)) = N'' THEN N'ELLE MÜDAHALE'
        ELSE c.CihazAdi
    END AS CihazAdi,
    k.Tarih,
    k.Tip,
    k.KayitZamani,
    k.AktifMi
FROM dbo.KisiHareketler AS k
LEFT JOIN dbo.Kisiler  AS p ON p.PersonelId = k.PersonelId
LEFT JOIN dbo.Cihazlar AS c ON c.CihazId   = k.CihazId
LEFT JOIN dbo.Firmalar AS f ON f.FirmaId   = k.FirmaId
WHERE k.FirmaId = @p0
  AND k.Tarih >= @p1
  AND k.Tarih <= @p2
");

            var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>
            {
                new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", bas),
                new Microsoft.Data.SqlClient.SqlParameter("@p2", bit)
            };

            if (onlyAktif && !onlyPasif)
                sb.AppendLine("  AND k.AktifMi = 1");
            else if (!onlyAktif && onlyPasif)
                sb.AppendLine("  AND k.AktifMi = 0");
            else if (!onlyAktif && !onlyPasif)
                sb.AppendLine("  AND k.AktifMi = 1");

            if (onlyYemekhane)
            {
                sb.AppendLine("  AND k.Tip = N'Yemekhane'");
            }
            else
            {
                sb.AppendLine("  AND (k.Tip IN (N'Giriş', N'Çıkış', N'Cikis'))");
            }

            if (personIds != null && personIds.Count > 0)
            {
                var inParams = new List<string>(personIds.Count);
                for (int i = 0; i < personIds.Count; i++)
                {
                    var pn = "@p" + (3 + i);
                    inParams.Add(pn);
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(pn, personIds[i]));
                }

                sb.Append("  AND k.PersonelId IN (");
                sb.Append(string.Join(",", inParams));
                sb.AppendLine(")");
            }

            sb.AppendLine("ORDER BY k.Tarih");

            string sql = sb.ToString();

            var rows = _context.Database
                .SqlQueryRaw<KisiHareketListRow>(sql, parameters.ToArray())
                .ToList();

            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Firma", typeof(string));
            dt.Columns.Add("SicilNo", typeof(string));
            dt.Columns.Add("AdSoyad", typeof(string));
            dt.Columns.Add("CihazAdi", typeof(string));
            dt.Columns.Add("Tarih", typeof(DateTime));
            dt.Columns.Add("Tip", typeof(string));
            dt.Columns.Add("KayitZamani", typeof(DateTime));
            dt.Columns.Add("AktifMi", typeof(bool));

            foreach (var r in rows)
            {
                dt.Rows.Add(r.Id, r.Firma, r.SicilNo, r.AdSoyad,
                            r.CihazAdi, r.Tarih, r.Tip, r.KayitZamani, r.AktifMi);
            }
            return dt;
        }

        public bool InsertManual(int firmaId, int personelId, DateTime tarih, string tip)
        {
            var entity = new CeyPASS.DataAccess.KisiHareketler
            {
                FirmaId = firmaId,
                PersonelId = personelId,
                Tarih = tarih,
                Tip = tip,
                KayitZamani = DateTime.Now,
                AktifMi = true,
                CihazId = 0
            };

            _context.KisiHareketler.Add(entity);
            return _context.SaveChanges() > 0;
        }

        public bool UpdateManual(int id, DateTime tarih, string tip)
        {
            var entity = _context.KisiHareketler
                .SingleOrDefault(k => k.Id == id);

            if (entity == null)
                return false;

            entity.Tarih = tarih;
            entity.Tip = tip;
            entity.CihazId = 0;
            entity.KayitZamani = DateTime.Now;

            return _context.SaveChanges() > 0;
        }

        public bool PasifYap(int id)
        {
            var entity = _context.KisiHareketler
                .SingleOrDefault(k => k.Id == id);

            if (entity == null)
                return false;

            entity.AktifMi = false;
            return _context.SaveChanges() > 0;
        }

        public DataTable GetAktifKisilerWithSicil(int firmaId)
        {
            var sql = @"
SELECT 
    PersonelId,
    Ad + ' ' + Soyad + ' [' + ISNULL(CAST(PersonelId AS nvarchar(50)), '') + ']' AS AdSoyad
FROM dbo.Kisiler
WHERE FirmaId = @p0
  AND PuantajYapilirMi = 1
  AND (IstenCikisTarihi IS NULL OR IstenCikisTarihi >= GETDATE())
ORDER BY Ad, Soyad";

            var rows = _context.Database
                .SqlQueryRaw<AktifKisiRow>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId))
                .ToList();

            var dt = new DataTable();
            dt.Columns.Add("PersonelId", typeof(string));
            dt.Columns.Add("AdSoyad", typeof(string));

            foreach (var r in rows)
            {
                dt.Rows.Add(r.PersonelId, r.AdSoyad);
            }

            return dt;
        }
    }
}
