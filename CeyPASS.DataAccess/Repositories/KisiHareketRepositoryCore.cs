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
    ISNULL(RTRIM(LTRIM(ISNULL(K.Ad, N'') + N' ' + ISNULL(K.Soyad, N''))), N'') AS AdSoyad,
    ISNULL(D.DepartmanAdi, N'') AS Departman,
    ISNULL(P.PozisyonAdi, N'')  AS Unvan,
    ISNULL(C.CihazAdi, N'')     AS CihazAdi,
    KH.PersonelId  AS PersonelId
FROM KisiHareketler KH
LEFT JOIN Kisiler         K  ON KH.PersonelId = K.PersonelId
LEFT JOIN Departmanlar    D  ON K.DepartmanId = D.DepartmanId
LEFT JOIN Cihazlar        C  ON KH.CihazId    = C.CihazId
LEFT JOIN Pozisyonlar     P  ON K.PozisyonId  = P.PozisyonId
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
    ISNULL(RTRIM(LTRIM(ISNULL(K.Ad, N'') + N' ' + ISNULL(K.Soyad, N''))), N'') AS AdSoyad,
    ISNULL(D.DepartmanAdi, N'') AS Departman,
    ISNULL(P.PozisyonAdi, N'')  AS Unvan,
    ISNULL(C.CihazAdi, N'')     AS CihazAdi,
    KH.PersonelId  AS PersonelId
FROM KisiHareketler KH
LEFT JOIN Kisiler         K  ON KH.PersonelId = K.PersonelId
LEFT JOIN Departmanlar    D  ON K.DepartmanId = D.DepartmanId
LEFT JOIN Cihazlar        C  ON KH.CihazId    = C.CihazId
LEFT JOIN Pozisyonlar     P  ON K.PozisyonId  = P.PozisyonId
WHERE C.FirmaId = @p1
  AND KH.Tip = N'Yemekhane'
ORDER BY KH.Tarih DESC";

            return _context.Database
                .SqlQueryRaw<KisiHareketDTO>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", top),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", firmaId))
                .ToList();
        }

        public List<KisiHareketDTO> GetLastMovesByFirmaArac(int top, int firmaId)
        {
            var sql = @"
SELECT TOP (@p0)
    KH.Tarih       AS Tarih,
    ISNULL(RTRIM(LTRIM(ISNULL(K.Ad, N'') + N' ' + ISNULL(K.Soyad, N''))), N'') AS AdSoyad,
    ISNULL(D.DepartmanAdi, N'') AS Departman,
    ISNULL(P.PozisyonAdi, N'')  AS Unvan,
    ISNULL(C.CihazAdi, N'')     AS CihazAdi,
    KH.PersonelId  AS PersonelId
FROM KisiHareketler KH
LEFT JOIN Kisiler         K  ON KH.PersonelId = K.PersonelId
LEFT JOIN Departmanlar    D  ON K.DepartmanId = D.DepartmanId
LEFT JOIN Cihazlar        C  ON KH.CihazId    = C.CihazId
LEFT JOIN Pozisyonlar     P  ON K.PozisyonId  = P.PozisyonId
WHERE C.FirmaId = @p1 AND C.AracGirisCikisMi=1
ORDER BY KH.Tarih DESC";

            return _context.Database
                .SqlQueryRaw<KisiHareketDTO>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", top),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", firmaId))
                .ToList();
        }

        public DataTable GetByPersons(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId)
        {
            bool sicilSecili = personIds != null && personIds.Count > 0;

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
    CASE
        WHEN k.Tip IN (N'G', N'Giriş', N'Giris') THEN N'Giriş'
        WHEN k.Tip IN (N'Ç', N'C', N'Çıkış', N'Cikis') THEN N'Çıkış'
        ELSE k.Tip
    END AS Tip,
    k.KayitZamani,
    k.AktifMi
FROM dbo.KisiHareketler AS k
LEFT JOIN dbo.Kisiler  AS p ON p.PersonelId = k.PersonelId
LEFT JOIN dbo.Cihazlar AS c ON c.CihazId   = k.CihazId
LEFT JOIN dbo.Firmalar AS f ON f.FirmaId   = k.FirmaId
WHERE ");

            var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>();
            int paramIndex = 0;

            if (sicilSecili)
            {
                sb.AppendLine("k.Tarih >= @p0");
                sb.AppendLine("  AND k.Tarih <= @p1");
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p0", bas));
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p1", bit));
                paramIndex = 2;
            }
            else
            {
                sb.AppendLine("k.FirmaId = @p0");
                sb.AppendLine("  AND k.Tarih >= @p1");
                sb.AppendLine("  AND k.Tarih <= @p2");
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId));
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p1", bas));
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p2", bit));
                paramIndex = 3;
            }

            AppendAktifPasifYemekhaneFilters(sb, onlyAktif, onlyPasif, onlyYemekhane);

            if (sicilSecili)
            {
                var inParams = new List<string>(personIds.Count);
                for (int i = 0; i < personIds.Count; i++)
                {
                    var pn = "@p" + (paramIndex + i);
                    inParams.Add(pn);
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(pn, personIds[i]));
                }

                sb.Append("  AND k.PersonelId IN (");
                sb.Append(string.Join(",", inParams));
                sb.AppendLine(")");
            }

            sb.AppendLine("ORDER BY k.Tarih DESC");

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

        public List<KisiHareketListRow> GetByPersonsPaged(List<int> personIds, DateTime bas, DateTime bit, bool onlyAktif, bool onlyPasif, bool onlyYemekhane, int firmaId, int page, int pageSize, out int totalCount)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            bool sicilSecili = personIds != null && personIds.Count > 0;

            var sbWhere = new StringBuilder(@"
FROM dbo.KisiHareketler AS k
LEFT JOIN dbo.Kisiler  AS p ON p.PersonelId = k.PersonelId
LEFT JOIN dbo.Cihazlar AS c ON c.CihazId   = k.CihazId
LEFT JOIN dbo.Firmalar AS f ON f.FirmaId   = k.FirmaId
WHERE ");

            var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>();
            int paramIndex = 0;

            if (sicilSecili)
            {
                sbWhere.AppendLine("k.Tarih >= @p0");
                sbWhere.AppendLine("  AND k.Tarih <= @p1");
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p0", bas));
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p1", bit));
                paramIndex = 2;
            }
            else
            {
                sbWhere.AppendLine("k.FirmaId = @p0");
                sbWhere.AppendLine("  AND k.Tarih >= @p1");
                sbWhere.AppendLine("  AND k.Tarih <= @p2");
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId));
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p1", bas));
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p2", bit));
                paramIndex = 3;
            }

            AppendAktifPasifYemekhaneFilters(sbWhere, onlyAktif, onlyPasif, onlyYemekhane);

            if (sicilSecili)
            {
                var inParams = new List<string>(personIds.Count);
                for (int i = 0; i < personIds.Count; i++)
                {
                    var pn = "@p" + (paramIndex + i);
                    inParams.Add(pn);
                    parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(pn, personIds[i]));
                }

                sbWhere.Append("  AND k.PersonelId IN (");
                sbWhere.Append(string.Join(",", inParams));
                sbWhere.AppendLine(")");
            }

            var countSql = "SELECT COUNT(1) " + sbWhere.ToString();
            totalCount = _context.Database
                .SqlQueryRaw<int>(countSql, parameters.ToArray())
                .AsEnumerable()
                .FirstOrDefault();

            var selectSql = @"
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
    CASE
        WHEN k.Tip IN (N'G', N'Giriş', N'Giris') THEN N'Giriş'
        WHEN k.Tip IN (N'Ç', N'C', N'Çıkış', N'Cikis') THEN N'Çıkış'
        ELSE k.Tip
    END AS Tip,
    k.KayitZamani,
    k.AktifMi
";

            var pageSql = new StringBuilder();
            pageSql.Append(selectSql);
            pageSql.Append(sbWhere);
            pageSql.AppendLine("ORDER BY k.Tarih DESC");
            pageSql.AppendLine("OFFSET @po ROWS FETCH NEXT @pf ROWS ONLY");
            parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@po", (page - 1) * pageSize));
            parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@pf", pageSize));

            return _context.Database
                .SqlQueryRaw<KisiHareketListRow>(pageSql.ToString(), parameters.ToArray())
                .ToList();
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

        public bool AktifYap(int id)
        {
            var entity = _context.KisiHareketler
                .SingleOrDefault(k => k.Id == id);

            if (entity == null)
                return false;

            entity.AktifMi = true;
            return _context.SaveChanges() > 0;
        }

        public DataTable GetAktifKisilerWithSicil(int firmaId, bool puantajYapilirMi = true)
        {
            var sql = @"
SELECT 
    PersonelId,
    Ad + ' ' + Soyad + ' [' + ISNULL(CAST(PersonelId AS nvarchar(50)), '') + ']' AS AdSoyad
FROM dbo.Kisiler
WHERE FirmaId = @p0
  AND PuantajYapilirMi = @p1
  AND (IstenCikisTarihi IS NULL OR IstenCikisTarihi >= GETDATE())
ORDER BY Ad, Soyad";

            var rows = _context.Database
                .SqlQueryRaw<AktifKisiRow>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", firmaId),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", puantajYapilirMi))
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

        /// <summary>
        /// Hiç checkbox yoksa AktifMi ve Tip filtresi uygulanmaz (tüm hareketler).
        /// Seçiliyse: Aktif/Pasif durum + turnike (G/Ç) ve/veya Yemekhane tipine göre daraltır.
        /// </summary>
        private static void AppendAktifPasifYemekhaneFilters(StringBuilder sb, bool onlyAktif, bool onlyPasif, bool onlyYemekhane)
        {
            bool anyFilter = onlyAktif || onlyPasif || onlyYemekhane;
            if (!anyFilter)
                return;

            if (onlyAktif && !onlyPasif)
                sb.AppendLine("  AND k.AktifMi = 1");
            else if (!onlyAktif && onlyPasif)
                sb.AppendLine("  AND k.AktifMi = 0");

            bool includeTurnike = onlyAktif || onlyPasif;
            bool includeYemek = onlyYemekhane;
            const string turnikeTips = "k.Tip IN (N'G', N'Ç', N'C', N'Giriş', N'Çıkış', N'Giris', N'Cikis')";

            if (includeTurnike && includeYemek)
                sb.AppendLine($"  AND ({turnikeTips} OR k.Tip = N'Yemekhane')");
            else if (includeYemek)
                sb.AppendLine("  AND k.Tip = N'Yemekhane'");
            else
                sb.AppendLine($"  AND ({turnikeTips})");
        }
    }
}
