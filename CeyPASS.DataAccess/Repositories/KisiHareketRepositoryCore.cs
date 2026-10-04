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
    /// <summary>KisiHareketler sorguları.</summary>
    public class KisiHareketRepositoryCore : IKisiHareketRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public KisiHareketRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        /// <summary>Last Moves By Firma sorgularını getirir.</summary>
        public List<KisiHareketDTO> GetLastMovesByFirma(int top, int firmaId)
        {
            var sql = @"
SELECT TOP (@p0)
    KH.Tarih       AS Tarih,
    ISNULL(RTRIM(LTRIM(ISNULL(K.Ad, N'') + N' ' + ISNULL(K.Soyad, N''))), N'') AS AdSoyad,
    ISNULL(I.IsyeriAdi, N'') AS Isyeri,
    ISNULL(P.PozisyonAdi, N'')  AS Unvan,
    ISNULL(C.CihazAdi, N'')     AS CihazAdi,
    KH.PersonelId  AS PersonelId
FROM KisiHareketler KH
LEFT JOIN Kisiler         K  ON KH.PersonelId = K.PersonelId
LEFT JOIN Isyerler         I  ON K.IsyeriId = I.IsyeriId AND K.FirmaId = I.FirmaId
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

        /// <summary>Last Moves By Firma Yemekhane sorgularını getirir.</summary>
        public List<KisiHareketDTO> GetLastMovesByFirmaYemekhane(int top, int firmaId)
        {
            var sql = @"
SELECT TOP (@p0)
    KH.Tarih       AS Tarih,
    ISNULL(RTRIM(LTRIM(ISNULL(K.Ad, N'') + N' ' + ISNULL(K.Soyad, N''))), N'') AS AdSoyad,
    ISNULL(I.IsyeriAdi, N'') AS Isyeri,
    ISNULL(P.PozisyonAdi, N'')  AS Unvan,
    ISNULL(C.CihazAdi, N'')     AS CihazAdi,
    KH.PersonelId  AS PersonelId
FROM KisiHareketler KH
LEFT JOIN Kisiler         K  ON KH.PersonelId = K.PersonelId
LEFT JOIN Isyerler         I  ON K.IsyeriId = I.IsyeriId AND K.FirmaId = I.FirmaId
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

        /// <summary>Last Moves By Firma Arac sorgularını getirir.</summary>
        public List<KisiHareketDTO> GetLastMovesByFirmaArac(int top, int firmaId)
        {
            var sql = @"
SELECT TOP (@p0)
    KH.Tarih       AS Tarih,
    ISNULL(RTRIM(LTRIM(ISNULL(K.Ad, N'') + N' ' + ISNULL(K.Soyad, N''))), N'') AS AdSoyad,
    ISNULL(I.IsyeriAdi, N'') AS Isyeri,
    ISNULL(P.PozisyonAdi, N'')  AS Unvan,
    ISNULL(C.CihazAdi, N'')     AS CihazAdi,
    KH.PersonelId  AS PersonelId
FROM KisiHareketler KH
LEFT JOIN Kisiler         K  ON KH.PersonelId = K.PersonelId
LEFT JOIN Isyerler         I  ON K.IsyeriId = I.IsyeriId AND K.FirmaId = I.FirmaId
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

        /// <summary>Last Giris Mi By Personel Ids sorgularını getirir.</summary>
        public List<PersonelSonHareketYon> GetLastGirisMiByPersonelIds(IReadOnlyList<string> personelIds)
        {
            if (personelIds == null || personelIds.Count == 0)
                return new List<PersonelSonHareketYon>();

            var intIds = new List<int>();
            var seen = new HashSet<int>();
            foreach (var raw in personelIds)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                if (!int.TryParse(raw.Trim(), out var id)) continue;
                if (seen.Add(id))
                    intIds.Add(id);
            }
            if (intIds.Count == 0)
                return new List<PersonelSonHareketYon>();

            var parameters = new List<object>();
            var inParams = new List<string>(intIds.Count);
            for (int i = 0; i < intIds.Count; i++)
            {
                var pn = "@p" + i;
                inParams.Add(pn);
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(pn, intIds[i]));
            }

            var inClause = string.Join(",", inParams);
            var sql = $@"
SELECT PersonelId, GirisMi, Tarih
FROM (
    SELECT
        CAST(KH.PersonelId AS nvarchar(50)) AS PersonelId,
        CASE
            WHEN KH.Tip = N'Giriş' THEN CAST(1 AS bit)
            WHEN KH.Tip = N'Yemekhane' THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END AS GirisMi,
        KH.Tarih AS Tarih,
        ROW_NUMBER() OVER (PARTITION BY KH.PersonelId ORDER BY KH.Tarih DESC) AS rn
    FROM KisiHareketler KH
    INNER JOIN Cihazlar C ON KH.CihazId = C.CihazId
    WHERE C.AnaGirisCikisMi = 1
      AND KH.PersonelId IN ({inClause})
) t
WHERE rn = 1";

            return _context.Database
                .SqlQueryRaw<PersonelSonHareketYon>(sql, parameters.ToArray())
                .ToList();
        }

        /// <summary>By Persons sorgularını getirir.</summary>
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
    k.AktifMi,
    k.ManuelMi
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
            dt.Columns.Add("ManuelMi", typeof(bool));

            foreach (var r in rows)
            {
                dt.Rows.Add(r.Id, r.Firma, r.SicilNo, r.AdSoyad,
                            r.CihazAdi, r.Tarih, r.Tip, r.KayitZamani, r.AktifMi, r.ManuelMi);
            }
            return dt;
        }

        /// <summary>By Persons Paged sorgularını getirir.</summary>
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
    k.AktifMi,
    k.ManuelMi
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

        /// <summary>Insert Manual işlemini ekler.</summary>
        public bool InsertManual(int firmaId, int personelId, DateTime tarih, string tip, int cihazId = 0)
        {
            var entity = new CeyPASS.DataAccess.KisiHareketler
            {
                FirmaId = firmaId,
                PersonelId = personelId,
                Tarih = tarih,
                Tip = tip,
                KayitZamani = DateTime.Now,
                AktifMi = true,
                CihazId = cihazId,
                ManuelMi = true
            };

            _context.KisiHareketler.Add(entity);
            return _context.SaveChanges() > 0;
        }

        /// <summary>Update Manual işlemini günceller.</summary>
        public bool UpdateManual(int id, DateTime tarih, string tip, int? cihazId = null)
        {
            var entity = _context.KisiHareketler
                .SingleOrDefault(k => k.Id == id);

            if (entity == null)
                return false;

            entity.Tarih = tarih;
            entity.Tip = tip;
            entity.ManuelMi = true;
            entity.KayitZamani = DateTime.Now;
            if (cihazId.HasValue)
                entity.CihazId = cihazId.Value;

            return _context.SaveChanges() > 0;
        }

        /// <inheritdoc />
        public PuantajGunHareketUctanUcaDTO GetGunUctanUca(int personelId, DateTime tarih)
        {
            var gunBas = tarih.Date;
            var gunBit = gunBas.AddDays(1);

            // Gece vardiyası: çıkış ertesi gün 12:00'a kadar olabilir
            var cikisBit = gunBas.AddDays(1).AddHours(12);

            var aktif = _context.KisiHareketler
                .Where(k => k.PersonelId == personelId
                            && k.AktifMi
                            && k.Tarih >= gunBas
                            && k.Tarih < cikisBit)
                .OrderBy(k => k.Tarih)
                .Select(k => new { k.Id, k.CihazId, k.Tarih, k.Tip })
                .ToList();

            var giris = aktif
                .Where(k => k.Tarih < gunBit
                            && (string.Equals(k.Tip, "Giriş", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(k.Tip, "Giris", StringComparison.OrdinalIgnoreCase)))
                .OrderBy(k => k.Tarih)
                .FirstOrDefault();

            var cikis = aktif
                .Where(k => string.Equals(k.Tip, "Çıkış", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(k.Tip, "Cikis", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(k => k.Tarih)
                .FirstOrDefault();

            return new PuantajGunHareketUctanUcaDTO
            {
                GirisHareketId = giris?.Id,
                GirisCihazId = giris?.CihazId,
                GirisTarih = giris?.Tarih,
                CikisHareketId = cikis?.Id,
                CikisCihazId = cikis?.CihazId,
                CikisTarih = cikis?.Tarih
            };
        }

        /// <summary>Pasif Yap işlemini gerçekleştirir.</summary>
        public bool PasifYap(int id)
        {
            var entity = _context.KisiHareketler
                .SingleOrDefault(k => k.Id == id);

            if (entity == null)
                return false;

            entity.AktifMi = false;
            return _context.SaveChanges() > 0;
        }

        /// <summary>Aktif Yap işlemini gerçekleştirir.</summary>
        public bool AktifYap(int id)
        {
            var entity = _context.KisiHareketler
                .SingleOrDefault(k => k.Id == id);

            if (entity == null)
                return false;

            entity.AktifMi = true;
            return _context.SaveChanges() > 0;
        }

        /// <summary>Aktif Kisiler With Sicil sorgularını getirir.</summary>
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
