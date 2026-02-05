using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    public class KisiIzinlerRepositoryCore : IKisiIzinlerRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public KisiIzinlerRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        public DataTable GetIzinleri(string personelId, DateTime baslangic, DateTime bitis)
        {
            var sql = @"
SELECT
    ki.KisiIzinId,
    ki.Baslangic                      AS IzinBaslangic,
    ki.Bitis                          AS IzinBitis,
    CASE 
      WHEN ki.SaatlikIzinMi = 1 THEN N'-'
      ELSE CONVERT(varchar(20), CAST(calc.GunNet AS decimal(10,2)))
    END                               AS SureGun,
    CAST(ki.SureDakika AS float) / 60.0 AS SureSaat,
    ki.Aciklama                        AS Aciklama,
    CONVERT(date, ki.OlusturmaTarihi)  AS IslenmeTarihi,
    CONVERT(date, ki.GuncellemeTarihi) AS GuncellemeTarihi,
    CASE WHEN ki.SaatlikIzinMi=1 THEN N'EVET' ELSE N'HAYIR' END AS SaatlikIzin
FROM dbo.KisiIzinler ki
CROSS APPLY (
    SELECT 
        S = CONVERT(date, ki.Baslangic),
        E = CONVERT(date, ki.Bitis)
) d
CROSS APPLY (
    SELECT
        ToplamGun = DATEDIFF(DAY, d.S, d.E) + 1,
        IlkPazar  = DATEADD(DAY, (7 - (DATEDIFF(DAY, '19000107', d.S) % 7)) % 7, d.S)
) t
CROSS APPLY (
    SELECT 
        Pazar = CASE 
                  WHEN t.IlkPazar > d.E THEN 0 
                  ELSE 1 + DATEDIFF(DAY, t.IlkPazar, d.E) / 7 
                END,
        RTAzalis = (
            SELECT ISNULL(SUM(
                CASE
                  WHEN (DATEDIFF(DAY,'19000107', rt.Tarih) % 7) = 0 THEN 0.0
                  WHEN rt.CalismaSaati >= 7.5 THEN 1.0
                  WHEN rt.CalismaSaati > 0 THEN rt.CalismaSaati / 7.5
                  ELSE 1.0
                END
            ),0.0)
            FROM dbo.ResmiTatiller rt
            WHERE rt.Tarih BETWEEN d.S AND d.E
        ),
        ToplamGun = t.ToplamGun
) calc0
CROSS APPLY (
    SELECT GunNet = CASE WHEN (calc0.ToplamGun - calc0.Pazar - calc0.RTAzalis) < 0
                         THEN 0.0
                         ELSE (calc0.ToplamGun - calc0.Pazar - calc0.RTAzalis)
                    END
) calc
WHERE ki.AktifMi = 1
  AND ki.PersonelId = @p0
  AND ki.Baslangic >= @p1 
  AND ki.Bitis     <= @p2
ORDER BY ki.Baslangic DESC";

            var rows = _context.Database
                .SqlQueryRaw<KisiIzinListRow>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId ?? (object)DBNull.Value),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", baslangic),
                    new Microsoft.Data.SqlClient.SqlParameter("@p2", bitis))
                .ToList();

            var dt = new DataTable();
            dt.Columns.Add("KisiIzinId", typeof(int));
            dt.Columns.Add("İzin Başlangıcı", typeof(DateTime));
            dt.Columns.Add("İzin Bitişi", typeof(DateTime));
            dt.Columns.Add("Süre(Gün)", typeof(string));
            dt.Columns.Add("Süre(Saat)", typeof(double));
            dt.Columns.Add("Açıklama", typeof(string));
            dt.Columns.Add("İşlenme Tarihi", typeof(DateTime));
            dt.Columns.Add("Güncelleme Tarihi", typeof(DateTime));
            dt.Columns.Add("Saatlik İzin", typeof(string));

            foreach (var r in rows)
            {
                dt.Rows.Add(
                    r.KisiIzinId,
                    r.IzinBaslangic,
                    r.IzinBitis,
                    r.SureGun,
                    r.SureSaat,
                    r.Aciklama,
                    (object)r.IslenmeTarihi ?? DBNull.Value,
                    (object)r.GuncellemeTarihi ?? DBNull.Value,
                    r.SaatlikIzin
                );
            }

            return dt;
        }

        public KisiIzin GetById(int kisiIzinId)
        {
            var entity = _context.KisiIzinler
                .FirstOrDefault(k => k.KisiIzinId == kisiIzinId && k.AktifMi == true);
            if (entity == null) return null;
            return new KisiIzin
            {
                KisiIzinId = entity.KisiIzinId,
                FirmaId = entity.FirmaId,
                PersonelId = entity.PersonelId,
                IzinId = entity.IzinId,
                Baslangic = entity.Baslangic,
                Bitis = entity.Bitis,
                SureDakika = entity.SureDakika.GetValueOrDefault(),
                Aciklama = entity.Aciklama,
                SaatlikIzinMi = entity.SaatlikIzinMi.GetValueOrDefault(),
                OlusturanKullaniciId = entity.OlusturanKullaniciId.GetValueOrDefault()
            };
        }

        public bool Insert(KisiIzin x)
        {
            var entity = new CeyPASS.DataAccess.KisiIzinler
            {
                FirmaId = x.FirmaId,
                PersonelId = x.PersonelId,
                IzinId = x.IzinId,
                Baslangic = x.Baslangic,
                Bitis = x.Bitis,
                Aciklama = x.Aciklama,
                SaatlikIzinMi = x.SaatlikIzinMi,
                OlusturanKullaniciId = x.OlusturanKullaniciId,
                OlusturmaTarihi = DateTime.Now,
                AktifMi = true
            };

            _context.KisiIzinler.Add(entity);
            _context.SaveChanges();

            x.KisiIzinId = entity.KisiIzinId;

            return entity.KisiIzinId > 0;
        }

        public bool Update(KisiIzin x)
        {
            if (!x.KisiIzinId.HasValue)
                return false;

            var entity = _context.KisiIzinler
                .FirstOrDefault(k => k.KisiIzinId == x.KisiIzinId.Value);

            if (entity == null)
                return false;

            entity.IzinId = x.IzinId;
            entity.Baslangic = x.Baslangic;
            entity.Bitis = x.Bitis;
            entity.Aciklama = x.Aciklama;
            entity.SaatlikIzinMi = x.SaatlikIzinMi;
            entity.GuncellemeTarihi = DateTime.Now;

            _context.SaveChanges();
            return true;
        }

        public bool PasifYap(int kisiIzinId)
        {
            var entity = _context.KisiIzinler
                .FirstOrDefault(k => k.KisiIzinId == kisiIzinId);

            if (entity == null)
                return false;

            entity.AktifMi = false;
            entity.GuncellemeTarihi = DateTime.Now;

            _context.SaveChanges();
            return true;
        }

        public DataTable GetByPerson(string personelId, DateTime? bas = null, DateTime? bit = null)
        {
            var sql = @"
SELECT
    KisiIzinId,
    Baslangic     AS IzinBaslangic,
    Bitis         AS IzinBitis,
    CAST(CAST(SureDakika AS decimal(10,2)) / 60.0 AS decimal(10,2)) AS SureSaat,
    Aciklama      AS Aciklama,
    OlusturmaTarihi  AS IslenmeTarihi,
    GuncellemeTarihi AS GuncellemeTarihi,
    CASE WHEN SaatlikIzinMi = 1 THEN N'EVET' ELSE N'HAYIR' END AS SaatlikIzin
FROM KisiIzinler
WHERE PersonelId = @p0 AND AktifMi = 1";

            var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>
            {
                new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId ?? (object)DBNull.Value)
            };

            if (bas.HasValue)
            {
                sql += " AND Baslangic >= @p1";
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p1", bas.Value));
            }
            if (bit.HasValue)
            {
                sql += " AND Bitis <= @p" + parameters.Count;
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p" + parameters.Count, bit.Value));
            }
            sql += " ORDER BY Baslangic DESC";

            var rows = _context.Database
                .SqlQueryRaw<KisiIzinByPersonRow>(sql, parameters.ToArray())
                .ToList();

            var dt = new DataTable();
            dt.Columns.Add("KisiIzinId", typeof(int));
            dt.Columns.Add("İzin Başlangıcı", typeof(DateTime));
            dt.Columns.Add("İzin Bitişi", typeof(DateTime));
            dt.Columns.Add("Süre(Saat)", typeof(decimal));
            dt.Columns.Add("Açıklama", typeof(string));
            dt.Columns.Add("İşlenme Tarihi", typeof(DateTime));
            dt.Columns.Add("Güncelleme Tarihi", typeof(DateTime));
            dt.Columns.Add("Saatlik İzin", typeof(string));

            foreach (var r in rows)
            {
                dt.Rows.Add(
                    r.KisiIzinId,
                    r.IzinBaslangic,
                    r.IzinBitis,
                    r.SureSaat,
                    r.Aciklama,
                    (object)r.IslenmeTarihi ?? DBNull.Value,
                    (object)r.GuncellemeTarihi ?? DBNull.Value,
                    r.SaatlikIzin
                );
            }
            return dt;
        }

        public DataTable GetIzinRaporu(int? firmaId, string personelId, int? izinTipId, DateTime bas, DateTime bit)
        {
            var sql = @"
SELECT
    ki.KisiIzinId,
    k.PersonelId                         AS SicilNo,
    (k.Ad + ' ' + k.Soyad)               AS AdSoyad,
    f.FirmaAdi                           AS FirmaAdi,
    it.Adi                               AS IzinTipi,
    ki.Baslangic                         AS BaslangicTarihi,
    ki.Bitis                             AS BitisTarihi,
    CASE 
      WHEN ki.SaatlikIzinMi = 1 THEN N'-'
      WHEN calc.GunNet = FLOOR(calc.GunNet)
           THEN CONVERT(varchar(20), CONVERT(int, calc.GunNet))
      ELSE REPLACE(CONVERT(varchar(20), CAST(ROUND(calc.GunNet, 2) AS decimal(10,2))), '.', ',')
    END                                          AS SureGun,
    REPLACE(
        CONVERT(varchar(20), CAST(ROUND(CAST(ki.SureDakika AS float) / 60.0, 2) AS decimal(10,2))),
        '.', ','
    )                                            AS SureSaat,
    ki.SaatlikIzinMi                     AS SaatlikIzinMi,
    ki.Aciklama                          AS Aciklama
FROM KisiIzinler ki
JOIN Kisiler      k  ON k.PersonelId = ki.PersonelId
JOIN Firmalar     f  ON f.FirmaId    = ki.FirmaId
JOIN IzinTipleri  it ON it.IzinTipId = ki.IzinId
CROSS APPLY (
    SELECT 
        S = CONVERT(date, ki.Baslangic),
        E = CONVERT(date, ki.Bitis)
) d
CROSS APPLY (
    SELECT
        ToplamGun = DATEDIFF(DAY, d.S, d.E) + 1,
        IlkPazar  = DATEADD(DAY, (7 - (DATEDIFF(DAY, '19000107', d.S) % 7)) % 7, d.S)
) t
CROSS APPLY (
    SELECT 
        Pazar = CASE 
                  WHEN t.IlkPazar > d.E THEN 0 
                  ELSE 1 + DATEDIFF(DAY, t.IlkPazar, d.E) / 7 
                END,
        RTAzalis = (
            SELECT ISNULL(SUM(
                CASE
                  WHEN (DATEDIFF(DAY,'19000107', rt.Tarih) % 7) = 0 THEN 0.0
                  WHEN rt.CalismaSaati >= 7.5 THEN 1.0
                  WHEN rt.CalismaSaati > 0 THEN rt.CalismaSaati / 7.5
                  ELSE 1.0
                END
            ),0.0)
            FROM dbo.ResmiTatiller rt
            WHERE rt.Tarih BETWEEN d.S AND d.E
        ),
        ToplamGun = t.ToplamGun
) calc0
CROSS APPLY (
    SELECT GunNet = CASE WHEN (calc0.ToplamGun - calc0.Pazar - calc0.RTAzalis) < 0
                         THEN 0.0
                         ELSE (calc0.ToplamGun - calc0.Pazar - calc0.RTAzalis)
                    END
) calc
WHERE ki.AktifMi = 1
  AND ki.Baslangic <= @p0
  AND ki.Bitis     >= @p1
";

            var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>
            {
                new Microsoft.Data.SqlClient.SqlParameter("@p0", bit),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", bas)
            };

            if (firmaId.HasValue)
            {
                sql += "  AND ki.FirmaId   = @p" + parameters.Count;
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p" + parameters.Count, firmaId.Value));
            }
            if (!string.IsNullOrEmpty(personelId))
            {
                sql += "  AND ki.PersonelId = @p" + parameters.Count;
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p" + parameters.Count, personelId));
            }
            if (izinTipId.HasValue)
            {
                sql += "  AND ki.IzinId    = @p" + parameters.Count;
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@p" + parameters.Count, izinTipId.Value));
            }

            sql += " ORDER BY ki.Baslangic";

            var rows = _context.Database
                .SqlQueryRaw<IzinRaporuRow>(sql, parameters.ToArray())
                .ToList();

            var dt = new DataTable();
            dt.Columns.Add("KisiIzinId", typeof(int));
            dt.Columns.Add("SicilNo", typeof(string));
            dt.Columns.Add("AdSoyad", typeof(string));
            dt.Columns.Add("FirmaAdi", typeof(string));
            dt.Columns.Add("IzinTipi", typeof(string));
            dt.Columns.Add("Başlangıç Tarihi", typeof(DateTime));
            dt.Columns.Add("Bitiş Tarihi", typeof(DateTime));
            dt.Columns.Add("Süre(Gün)", typeof(string));
            dt.Columns.Add("Süre(Saat)", typeof(string));
            dt.Columns.Add("Saatlik İzin Mi", typeof(string));
            dt.Columns.Add("Açıklama", typeof(string));
            dt.Columns.Add("IslenmeTarihi", typeof(DateTime));
            dt.Columns.Add("GuncellemeTarihi", typeof(DateTime));

            foreach (var r in rows)
            {
                dt.Rows.Add(
                    r.KisiIzinId,
                    r.SicilNo ?? "",
                    r.AdSoyad ?? "",
                    r.FirmaAdi ?? "",
                    r.IzinTipi ?? "",
                    r.BaslangicTarihi,
                    r.BitisTarihi,
                    r.SureGun ?? "",
                    r.SureSaat ?? "",
                    r.SaatlikIzinMi ? "EVET" : "HAYIR",
                    r.Aciklama ?? "",
                    DBNull.Value,
                    DBNull.Value
                );
            }
            return dt;
        }
    }
}
