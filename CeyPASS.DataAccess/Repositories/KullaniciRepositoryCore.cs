using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    /// <summary>Kullanicilar erişimi.</summary>
    public class KullaniciRepositoryCore : IKullaniciRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public KullaniciRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        private static Kullanici MapToKullanici(KullaniciQueryRow row)
        {
            if (row == null) return null;

            int kullaniciIdInt = 0;
            if (!string.IsNullOrWhiteSpace(row.KullaniciId))
                int.TryParse(row.KullaniciId, out kullaniciIdInt);

            return new Kullanici
            {
                KullaniciId = kullaniciIdInt,
                KullaniciAdi = row.KullaniciAdi,
                Sifre = row.Sifre,
                RolId = row.RolId,
                PersonelId = row.PersonelId,
                RolTanimi = row.RolTanimi,
                AdSoyad = row.AdSoyad,
                FirmaId = row.FirmaId,
                FirmaAdi = row.FirmaAdi,
                Email = row.Email
            };
        }

        /// <summary>Kullanici Dogrula işlemini gerçekleştirir.</summary>
        public Kullanici KullaniciDogrula(string kullaniciAdi, string sifre)
        {
            const string sql = @"
SELECT TOP 1
       k.KullaniciId,
       k.KullaniciAdi,
       k.Sifre,
       k.RolId,
       k.PersonelId,
       r.RolTanimi,
       LTRIM(RTRIM(CONCAT(kis.Ad, ' ', kis.Soyad))) AS AdSoyad,
       kis.FirmaId,
       f.FirmaAdi,
       kis.Email
FROM   Kullanicilar k
LEFT JOIN Roller   r   ON k.RolId      = r.RolId
LEFT JOIN Kisiler  kis ON CAST(k.PersonelId AS NVARCHAR(50)) = kis.PersonelId
LEFT JOIN Firmalar f   ON kis.FirmaId  = f.FirmaId
WHERE  k.KullaniciAdi = {0}
  AND  k.Sifre        = {1}";

            var row = _context.Database
                .SqlQueryRaw<KullaniciQueryRow>(sql, kullaniciAdi, sifre)
                .FirstOrDefault();

            return MapToKullanici(row);
        }

        /// <summary>Sifre Guncelle işlemini gerçekleştirir.</summary>
        public bool SifreGuncelle(string kullaniciAdi, string yeniSifre)
        {
            const string sql = @"
UPDATE dbo.Kullanicilar
SET Sifre = {1}
WHERE KullaniciAdi = {0}";

            var affected = _context.Database.ExecuteSqlRaw(sql, kullaniciAdi, yeniSifre);
            return affected > 0;
        }

        /// <summary>Kullaniciya Kod Gonder işlemini gerçekleştirir.</summary>
        public string KullaniciyaKodGonder(string kullaniciAdi)
        {
            const string sql = @"
SELECT TOP 1 ki.Email
FROM   Kullanicilar k
JOIN   Kisiler      ki ON CAST(k.PersonelId AS NVARCHAR(50)) = ki.PersonelId
WHERE  k.KullaniciAdi = {0}";

            return _context.Database
                .SqlQueryRaw<string>(sql, kullaniciAdi)
                .FirstOrDefault();
        }

        /// <summary>Isyeri Id List By Firma sorgularını getirir.</summary>
        public List<int> GetIsyeriIdListByFirma(int firmaId)
        {
            const string sql = @"
SELECT IsyeriId
FROM dbo.Isyerler
WHERE FirmaId = {0} AND IsyeriId IS NOT NULL";

            return _context.Database
                .SqlQueryRaw<int>(sql, firmaId)
                .ToList();
        }

        /// <summary>By User Name sorgularını getirir.</summary>
        public Kullanici GetByUserName(string kullaniciAdi)
        {
            const string sql = @"
SELECT TOP 1
       U.KullaniciId,
       U.KullaniciAdi,
       U.Sifre,
       U.RolId,
       U.PersonelId,
       R.RolTanimi,
       LTRIM(RTRIM(CONCAT(K.Ad, ' ', K.Soyad))) AS AdSoyad,
       K.FirmaId,
       F.FirmaAdi,
       K.Email
FROM   [CeyPASS].[dbo].[Kullanicilar] AS U
INNER JOIN Roller   AS R ON U.RolId      = R.RolId
INNER JOIN Kisiler  AS K ON CAST(U.PersonelId AS NVARCHAR(50)) = K.PersonelId
LEFT  JOIN Firmalar AS F ON K.FirmaId    = F.FirmaId
WHERE  U.KullaniciAdi = {0}";

            var row = _context.Database
                .SqlQueryRaw<KullaniciQueryRow>(sql, kullaniciAdi)
                .FirstOrDefault();

            return MapToKullanici(row);
        }

        /// <summary>By Personel Id sorgularını getirir.</summary>
        public Kullanici GetByPersonelId(string personelId)
        {
            const string sql = @"
SELECT TOP 1
       U.KullaniciId,
       U.KullaniciAdi,
       U.Sifre,
       U.RolId,
       U.PersonelId,
       R.RolTanimi,
       LTRIM(RTRIM(CONCAT(K.Ad, ' ', K.Soyad))) AS AdSoyad,
       K.FirmaId,
       F.FirmaAdi,
       K.Email
FROM   [CeyPASS].[dbo].[Kullanicilar] AS U
INNER JOIN Roller   AS R ON U.RolId      = R.RolId
INNER JOIN Kisiler  AS K ON CAST(U.PersonelId AS NVARCHAR(50)) = K.PersonelId
LEFT  JOIN Firmalar AS F ON K.FirmaId    = F.FirmaId
WHERE  K.PersonelId = {0}";

            var row = _context.Database
                .SqlQueryRaw<KullaniciQueryRow>(sql, personelId)
                .FirstOrDefault();

            return MapToKullanici(row);
        }

        /// <summary>Kurtarma Kodu Kaydet işlemini gerçekleştirir.</summary>
        public void KurtarmaKoduKaydet(int kullaniciId, string kod, DateTime sonKullanmaZamani)
        {
            const string sql = @"
INSERT INTO dbo.KullaniciSifreKurtarma
(KullaniciId, KurtarmaKodu, SonKullanmaZamani, Kullanildi)
VALUES
({0}, {1}, {2}, 0)";

            _context.Database.ExecuteSqlRaw(sql, kullaniciId, kod, sonKullanmaZamani);
        }

        /// <summary>Kurtarma Kodu sorgularını getirir.</summary>
        public string GetKurtarmaKodu(int kullaniciId)
        {
            const string sql = @"
SELECT TOP 1 KurtarmaKodu
FROM dbo.KullaniciSifreKurtarma
WHERE KullaniciId = {0} AND Kullanildi = 0
ORDER BY Id DESC";

            return _context.Database
                .SqlQueryRaw<string>(sql, kullaniciId)
                .FirstOrDefault();
        }

        /// <summary>Kurtarma Kodunu Temizle işlemini gerçekleştirir.</summary>
        public void KurtarmaKodunuTemizle(int kullaniciId)
        {
            const string sql = @"
UPDATE dbo.KullaniciSifreKurtarma
SET Kullanildi = 1
WHERE KullaniciId = {0} AND Kullanildi = 0";

            _context.Database.ExecuteSqlRaw(sql, kullaniciId);
        }

        /// <summary>Giriş ekranı dropdown için tüm kullanıcı adlarını getirir (yetkili tablosu).</summary>
        public List<string> GetTumKullaniciAdlari()
        {
            return _context.Kullanicilar
                .OrderBy(k => k.KullaniciAdi)
                .Select(k => k.KullaniciAdi)
                .ToList();
        }

        /// <summary>Admin User Ids sorgularını getirir.</summary>
        public List<string> GetAdminUserIds()
        {
            // RolId = 1 genellikle Admin veya Süper Admin'dir. 
            // Burada RolId = 1 olan tüm KullaniciId'leri dönüyoruz.
            return _context.Kullanicilar
                .Where(k => k.RolId == 1)
                .Select(k => k.KullaniciId)
                .ToList();
        }
    }
}
