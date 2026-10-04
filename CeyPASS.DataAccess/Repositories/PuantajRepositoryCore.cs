using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    /// <summary>Puantaj onay, final veri ve aylık toplu sorgular.</summary>
    public class PuantajRepositoryCore : IPuantajRepository
    {
        private readonly CeyPASSDataConnectionCore _context;
        private readonly IKullaniciFirmaIsyeriYetkiRepository _yetkiRepo;

        public PuantajRepositoryCore(CeyPASSDataConnectionCore context, IKullaniciFirmaIsyeriYetkiRepository yetkiRepo)
        {
            _context = context;
            _yetkiRepo = yetkiRepo;
        }

        /// <summary>sp_AylikPuantajVeri ile günlük puantaj özet satırlarını okur; onay bilgisi PuantajOnay ile birleştirilir.</summary>
        public List<PuantajGunSatirDTO> SpPuantajAyOzet(int personelId, int yil, int ay)
        {
            var baslangic = new DateTime(yil, ay, 1);
            var bitisSonrasi = baslangic.AddMonths(1);
            var onaylar = _context.PuantajOnay
                .Where(p => p.PersonelId == personelId && p.Tarih >= baslangic && p.Tarih < bitisSonrasi)
                .Select(p => new { p.Tarih, p.OnayDurumu, p.Aciklama, p.DuzenlenmisFMDakika })
                .ToList();

            var list = new List<PuantajGunSatirDTO>();
            var conn = _context.Database.GetDbConnection();
            var shouldClose = conn.State != ConnectionState.Open;
            try
            {
                if (shouldClose) conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "dbo.sp_AylikPuantajVeri";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@PersonelId", personelId));
                cmd.Parameters.Add(new SqlParameter("@Yil", yil));
                cmd.Parameters.Add(new SqlParameter("@Ay", ay));

                using (var reader = cmd.ExecuteReader())
                {
                static string NormalizeCol(string s)
                {
                    if (string.IsNullOrWhiteSpace(s)) return "";
                    s = s.Trim().ToLowerInvariant();
                    s = s.Replace(" ", "").Replace("_", "");
                    s = s.Replace('ı', 'i').Replace('İ', 'i')
                         .Replace('ş', 's').Replace('Ş', 's')
                         .Replace('ğ', 'g').Replace('Ğ', 'g')
                         .Replace('ü', 'u').Replace('Ü', 'u')
                         .Replace('ö', 'o').Replace('Ö', 'o')
                         .Replace('ç', 'c').Replace('Ç', 'c');
                    return s;
                }

                var ordinalsByNorm = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var key = NormalizeCol(reader.GetName(i));
                    if (!ordinalsByNorm.ContainsKey(key))
                        ordinalsByNorm[key] = i;
                }

                int FindOrdinal(params string[] candidates)
                {
                    foreach (var c in candidates ?? Array.Empty<string>())
                    {
                        if (string.IsNullOrWhiteSpace(c)) continue;
                        try
                        {
                            return reader.GetOrdinal(c);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            var norm = NormalizeCol(c);
                            if (ordinalsByNorm.TryGetValue(norm, out var idx))
                                return idx;
                        }
                    }
                    return -1;
                }

                var oTarih = FindOrdinal("Tarih");
                var oVardiyaTuru = FindOrdinal(
                    "VardiyaTuru", "Vardiya", "Vardiya Türü", "Vardiya Turu", "VardiyaTipi",
                    "VardiyaAdi", "VardiyaAd", "VardiyaKodu", "VardiyaKod", "VardiyaTur", "VardiyaTurAdi");
                var oIlkGiris = FindOrdinal(
                    "IlkGiris", "İlk Giriş", "Ilk Giris", "IlkGirisSaat", "IlkGirisZamani", "IlkGirisZaman",
                    "Giris", "Giriş", "GirisSaat", "GirisZamani", "GirisZaman", "Giris1", "GirisSaat1", "IlkGirisSaati");
                var oSonCikis = FindOrdinal(
                    "SonCikis", "Son Çıkış", "Son Cikis", "SonCikisSaat", "SonCikisZamani", "SonCikisZaman",
                    "Cikis", "Çıkış", "CikisSaat", "CikisZamani", "CikisZaman", "Cikis1", "CikisSaat1", "SonCikisSaati");
                var oVardiyaBaslangic = FindOrdinal(
                    "VardiyaBaslangic", "Vardiya Başlangıç", "Vardiya Baslangic", "VardiyaBaslangicSaat", "VardiyaBaslangicZamani", "VardiyaBaslangicZaman",
                    "VardiyaBaslama", "VardiyaBaslamaSaati", "VardiyaBaslangicSaati",
                    "PlanBaslangic", "PlanBaslama", "BaslangicSaat", "ShiftStart", "VardiyaStart");
                var oVardiyaBitis = FindOrdinal(
                    "VardiyaBitis", "Vardiya Bitiş", "Vardiya Bitis", "VardiyaBitisSaat", "VardiyaBitisZamani", "VardiyaBitisZaman",
                    "VardiyaBitisSaati", "VardiyaBitirme",
                    "PlanBitis", "PlanCikis", "BitisSaat", "ShiftEnd", "VardiyaEnd");
                var oOnayDurumu = FindOrdinal("OnayDurumu", "Onay Durumu");
                var oDuzenlenmisFm = FindOrdinal("DuzenlenmisFMDakika", "Düzeltilmiş FM", "DuzenlenmisFM", "DuzenlenmisFMDk", "DuzenlenmisFmDakika");
                var oAciklama = FindOrdinal("Aciklama", "Açıklama");
                var oCalismaTipi = FindOrdinal("CalismaTipi", "Çalışma Tipi", "Calisma Tipi", "CalismaTip");
                var oSaat = FindOrdinal("Saat", "Çalışma Saati", "CalismaSaati", "Calisma Saat", "CalismaSaat");
                var oSaatlikIzin = FindOrdinal("SaatlikIzinDakika", "Saatlik İzin", "SaatlikIzin", "SaatlikIzinDk", "SaatlikIzinDak");

                TimeSpan? ReadTime(int ordinal)
                {
                    if (ordinal < 0 || reader.IsDBNull(ordinal)) return null;
                    var val = reader.GetValue(ordinal);
                    return val switch
                    {
                        TimeSpan ts => ts,
                        DateTime dt => dt.TimeOfDay,
                        _ => TimeSpan.TryParse(val.ToString(), out var parsed) ? parsed : null
                    };
                }

                int? ReadInt(int ordinal)
                {
                    if (ordinal < 0 || reader.IsDBNull(ordinal)) return null;
                    return Convert.ToInt32(reader.GetValue(ordinal));
                }

                decimal? ReadDecimal(int ordinal)
                {
                    if (ordinal < 0 || reader.IsDBNull(ordinal)) return null;
                    var val = reader.GetValue(ordinal);
                    if (val is string s && !string.IsNullOrWhiteSpace(s))
                    {
                        s = s.Trim();
                        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.GetCultureInfo("tr-TR"), out var tr))
                            return tr;
                        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var inv))
                            return inv;
                    }
                    return Convert.ToDecimal(val);
                }

                string ReadString(int ordinal)
                {
                    if (ordinal < 0 || reader.IsDBNull(ordinal)) return "";
                    return Convert.ToString(reader.GetValue(ordinal)) ?? "";
                }

                while (reader.Read())
                {
                    if (oTarih < 0 || reader.IsDBNull(oTarih)) continue;
                    var tarih = Convert.ToDateTime(reader.GetValue(oTarih));

                    var onayInt = ReadInt(oOnayDurumu);

                    list.Add(new PuantajGunSatirDTO
                    {
                        Tarih = tarih,
                        VardiyaTuru = ReadString(oVardiyaTuru),
                        IlkGiris = ReadTime(oIlkGiris),
                        SonCikis = ReadTime(oSonCikis),
                        VardiyaBaslangic = ReadTime(oVardiyaBaslangic),
                        VardiyaBitis = ReadTime(oVardiyaBitis),
                        OnayDurumu = onayInt.HasValue ? (OnayDurumu)onayInt.Value : OnayDurumu.Bekliyor,
                        DuzenlenenFMDakika = ReadInt(oDuzenlenmisFm) ?? 0,
                        Aciklama = ReadString(oAciklama),
                        CalismaTipi = ReadString(oCalismaTipi),
                        Saat = NormalizeCalismaSaati(ReadDecimal(oSaat) ?? 0m),
                        SaatlikIzinDakika = ReadInt(oSaatlikIzin) ?? 0,
                        ErkenGirisDakika = 0,
                        GecCikisDakika = 0,
                        SistemFMDakika = 0
                    });
                }

                }
            }
            finally
            {
                if (shouldClose) conn.Close();
            }

            foreach (var item in list)
            {
                var itemTarih = item.Tarih.Date;
                var onay = onaylar.FirstOrDefault(o => o.Tarih.Date == itemTarih);
                if (onay != null)
                {
                    item.OnayDurumu = (OnayDurumu)onay.OnayDurumu;
                    if (onay.Aciklama != null) item.Aciklama = onay.Aciklama;
                    item.DuzenlenenFMDakika = onay.DuzenlenmisFMDakika ?? 0;
                }
            }

            return list;
        }

        private static decimal NormalizeCalismaSaati(decimal value)
        {
            if (value <= 24m) return value;
            if (value <= 99m) return value / 10m;   // 75 -> 7,5
            return value / 100m;                     // 750 -> 7,5
        }

        /// <summary>Günlük puantaj onay kaydını sp_Puantaj_Onay_Upsert ile yazar/günceller.</summary>
        public void Sp_OnayUpsert(object con, int personelId, DateTime tarih, int onayDurumu, int duzenlenmisFm, string aciklama, int? kullaniciId)
        {
            var sql = "EXEC dbo.sp_Puantaj_Onay_Upsert @p0, @p1, @p2, @p3, @p4, @p5";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", tarih.Date),
                new Microsoft.Data.SqlClient.SqlParameter("@p2", onayDurumu),
                new Microsoft.Data.SqlClient.SqlParameter("@p3", duzenlenmisFm),
                new Microsoft.Data.SqlClient.SqlParameter("@p4", aciklama ?? (object)DBNull.Value),
                new Microsoft.Data.SqlClient.SqlParameter("@p5", kullaniciId ?? (object)DBNull.Value));
        }

        /// <summary>Final puantaj satırını sp_Puantaj_Final_Upsert ile yazar/günceller.</summary>
        public void Sp_FinalUpsert(object con, int personelId, DateTime tarih, string calismaTipi, decimal saat, int? kullaniciId)
        {
            var sql = "EXEC dbo.sp_Puantaj_Final_Upsert @p0, @p1, @p2, @p3, @p4";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", personelId),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", tarih.Date),
                new Microsoft.Data.SqlClient.SqlParameter("@p2", calismaTipi ?? (object)DBNull.Value),
                new Microsoft.Data.SqlClient.SqlParameter("@p3", saat),
                new Microsoft.Data.SqlClient.SqlParameter("@p4", kullaniciId ?? (object)DBNull.Value));
        }

        /// <summary>Onay ve final puantaj yazımını tek transaction içinde uygular.</summary>
        public void ApproveAndWriteFinal(int personelId, DateTime tarih, int onayDurumu, int duzenlenmisFm, string aciklama, string calismaTipi, decimal saat, int? kullaniciId)
        {
            void Write()
            {
                Sp_OnayUpsert(null, personelId, tarih, onayDurumu,
                              duzenlenmisFm, aciklama, kullaniciId);

                Sp_FinalUpsert(null, personelId, tarih, calismaTipi,
                               saat, kullaniciId);
            }

            if (_context.Database.CurrentTransaction != null)
            {
                Write();
                return;
            }

            using (var tx = _context.Database.BeginTransaction())
            {
                try
                {
                    Write();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        /// <summary>Yalnızca onay kaydını transaction ile yazar/günceller.</summary>
        public void OnayUpsert(int personelId, DateTime tarih, int onayDurumu, int duzenlenmisFm, string aciklama, int? kullaniciId)
        {
            using (var tx = _context.Database.BeginTransaction())
            {
                try
                {
                    Sp_OnayUpsert(null, personelId, tarih, onayDurumu,
                                  duzenlenmisFm, aciklama, kullaniciId);
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        /// <summary>Aktif puantaj tiplerini sp_PuantajTipleri_GetActive ile getirir.</summary>
        public List<PuantajTipDTO> GetPuantajTipleri()
        {
            var sql = "EXEC dbo.sp_PuantajTipleri_GetActive";

            return _context.Database
                .SqlQueryRaw<PuantajTipDTO>(sql)
                .ToList();
        }

        /// <summary>Ana sicil puantajını bağlı hedef sicillere sp_CokluSicileAktar ile kopyalar.</summary>
        public void CokluSicileAktar(int anaKey, int yil, int ay, int? kullaniciId)
        {
            var sql = "EXEC dbo.sp_CokluSicileAktar @p0, @p1, @p2, @p3";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", anaKey),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", yil),
                new Microsoft.Data.SqlClient.SqlParameter("@p2", ay),
                new Microsoft.Data.SqlClient.SqlParameter("@p3", kullaniciId ?? (object)DBNull.Value));
        }

        /// <summary>Ana sicile bağlı aktif hedef sicil sayısını döner.</summary>
        public int GetHedefSicilSayisi(int anaSicilNo)
        {
            return _context.CokluSicilBaglantilari
                .Count(x => x.AnaPersonelId == anaSicilNo &&
                            x.AktifMi);
        }

        /// <summary>Sicilin aktif çoklu sicil ana kaydı olup olmadığını kontrol eder.</summary>
        public bool IsAnaSicil(int sicilNo)
        {
            return _context.CokluSicilBaglantilari
                .Any(x => x.AnaPersonelId == sicilNo &&
                          x.AktifMi);
        }

        /// <summary>Sistem ayarı EkKayitGun değerini okur (geçmiş ay ek kayıt penceresi).</summary>
        public int GetEkKayitGun()
        {
            var sql = "EXEC dbo.sp_Ayar_Get @p0";

            var val = _context.Database
                .SqlQueryRaw<string>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", "EkKayitGun"))
                .AsEnumerable()
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(val))
                return 0;

            return int.TryParse(val, out var gun) ? gun : 0;
        }

        /// <summary>EkKayitGun sistem ayarını günceller.</summary>
        public void SetEkKayitGun(int gun, int? kullaniciId)
        {
            var sql = "EXEC dbo.sp_Ayar_Set @p0, @p1, @p2";

            _context.Database.ExecuteSqlRaw(sql,
                new Microsoft.Data.SqlClient.SqlParameter("@p0", "EkKayitGun"),
                new Microsoft.Data.SqlClient.SqlParameter("@p1", gun.ToString()),
                new Microsoft.Data.SqlClient.SqlParameter("@p2", kullaniciId ?? (object)DBNull.Value));
        }

        /// <summary>Kullanıcının firma/işyeri yetki listesini döner.</summary>
        public List<FirmaIsyeriYetkiDTO> GetKullaniciFirmaIsyeriYetkileri(int kullaniciId)
            => _yetkiRepo.GetYetkiler(kullaniciId);

        /// <summary>Seçilen ay için puantaj yapılacak sicil listesini döner. PuantajYapilirMi, işe giriş-çıkış ay penceresi, aktif CokluSicilBaglantilari (hedef siciller) ve kullanıcı Firma/İşyeri yetkileri birleştirilir; ana sicil çoklu bağlıysa BaseOnly dışlanır.</summary>
        public DataTable GetSicillerAyIcin(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler)
        {
            string yetkilerValues = string.Join(",", yetkiler.Select(y =>
                $"({y.FirmaId},{(y.IsyeriId.HasValue ? y.IsyeriId.Value.ToString() : "NULL")})"
            ));

            var sql = $@"
DECLARE @Yil int = @p0;
DECLARE @Ay int = @p1;
DECLARE @AyBas date = DATEFROMPARTS(@Yil, @Ay, 1);
DECLARE @AySon date = EOMONTH(@AyBas);

-- Kullanıcının görebileceği firma/işyeri çiftleri (IsyeriId NULL = firmadaki tüm işyerleri)
DECLARE @Yetkiler TABLE (FirmaId INT, IsyeriId INT NULL);
INSERT INTO @Yetkiler (FirmaId, IsyeriId)
VALUES {yetkilerValues};

-- HedefMap: Çoklu sicil bağlantısındaki hedef siciller; puantaj satırı bağlantıdaki firma/işyeri ile listelenir
;WITH HedefRaw AS (
    SELECT
        c.HedefPersonelId AS SicilNo,
        k.TcKimlikNo,
        k.Ad,
        k.Soyad,
        c.FirmaId AS Firma,
        c.SirketId AS Isyeri,
        c.BolumId AS Bolum,
        COALESCE(c.IseGirisTarihi, k.IseGirisTarihi) AS IseGirisTarihi,
        COALESCE(c.IstenCikisTarihi, k.IstenCikisTarihi) AS IstenCikisTarihi,
        CASE WHEN k.CalismaStatusu = 3 THEN 1 ELSE 0 END AS DokPersoneliMi,
        ROW_NUMBER() OVER (
            PARTITION BY c.HedefPersonelId
            ORDER BY c.FirmaId, c.SirketId
        ) AS rn
    FROM dbo.CokluSicilBaglantilari c
    JOIN dbo.Kisiler k ON k.PersonelId = c.HedefPersonelId
    WHERE c.AktifMi = 1
      AND k.PuantajYapilirMi = 1  -- Puantajsız / izlenmeyecek personeli dışla
      AND COALESCE(c.IseGirisTarihi, k.IseGirisTarihi) <= @AySon
      AND (COALESCE(c.IstenCikisTarihi, k.IstenCikisTarihi) IS NULL
           OR COALESCE(c.IstenCikisTarihi, k.IstenCikisTarihi) >= @AyBas)  -- Seçilen ayda en az bir gün aktif
      -- Yetki: kullanıcı yalnızca yetkili olduğu firma/işyerindeki bağlantıları görür
      AND EXISTS (
          SELECT 1 FROM @Yetkiler y 
          WHERE y.FirmaId = c.FirmaId 
            AND (y.IsyeriId IS NULL OR y.IsyeriId = c.SirketId)
      )
),
HedefMap AS (
    SELECT
        SicilNo, TcKimlikNo, Ad, Soyad, Firma, Isyeri, Bolum,
        IseGirisTarihi, IstenCikisTarihi, DokPersoneliMi
    FROM HedefRaw
    WHERE rn = 1
),
-- BaseOnly: Aktif çoklu sicil hedefi olmayan normal siciller (hedefler HedefMap'te ayrı satır)
BaseOnly AS (
    SELECT
        k.PersonelId AS SicilNo,
        k.TcKimlikNo,
        k.Ad,
        k.Soyad,
        k.FirmaId AS Firma,
        k.IsyeriId AS Isyeri,
        k.BolumId AS Bolum,
        k.IseGirisTarihi,
        k.IstenCikisTarihi,
        CASE WHEN k.CalismaStatusu = 3 THEN 1 ELSE 0 END AS DokPersoneliMi
    FROM dbo.Kisiler k
    WHERE k.PuantajYapilirMi = 1
      AND NOT EXISTS (
            SELECT 1
            FROM dbo.CokluSicilBaglantilari c
            WHERE c.HedefPersonelId = k.PersonelId AND c.AktifMi = 1
      )  -- Hedef sicil ana Kisiler satırında tekrar listelenmesin
      AND k.IseGirisTarihi <= @AySon
      AND (k.IstenCikisTarihi IS NULL OR k.IstenCikisTarihi >= @AyBas)
      -- Yetki: firma/işyeri kullanıcı kapsamı
      AND EXISTS (
          SELECT 1 FROM @Yetkiler y 
          WHERE y.FirmaId = k.FirmaId 
            AND (y.IsyeriId IS NULL OR y.IsyeriId = k.IsyeriId)
      )
)
SELECT * FROM BaseOnly
UNION ALL
SELECT * FROM HedefMap
ORDER BY SicilNo";

            var rows = _context.Database
                .SqlQueryRaw<SicilAyRow>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", yil),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", ay))
                .ToList();

            var dt = new DataTable();
            dt.Columns.Add("SicilNo", typeof(int));
            dt.Columns.Add("TcKimlikNo", typeof(string));
            dt.Columns.Add("Ad", typeof(string));
            dt.Columns.Add("Soyad", typeof(string));
            dt.Columns.Add("Firma", typeof(int));
            dt.Columns.Add("Isyeri", typeof(int));
            dt.Columns.Add("Bolum", typeof(int));
            dt.Columns.Add("IseGirisTarihi", typeof(DateTime));
            dt.Columns.Add("IstenCikisTarihi", typeof(DateTime));
            dt.Columns.Add("DokPersoneliMi", typeof(int));

            foreach (var r in rows)
            {
                dt.Rows.Add(
                    r.SicilNo,
                    r.TcKimlikNo,
                    r.Ad,
                    r.Soyad,
                    r.Firma.HasValue ? (object)r.Firma.Value : DBNull.Value,
                    r.Isyeri.HasValue ? (object)r.Isyeri.Value : DBNull.Value,
                    r.Bolum.HasValue ? (object)r.Bolum.Value : DBNull.Value,
                    r.IseGirisTarihi.HasValue ? (object)r.IseGirisTarihi.Value : DBNull.Value,
                    r.IstenCikisTarihi.HasValue ? (object)r.IstenCikisTarihi.Value : DBNull.Value,
                    r.DokPersoneliMi
                );
            }

            return dt;
        }

        /// <summary>Ay içindeki FinalPuantajVerisi satırlarını yetkiye göre filtreler. Sicilin firma/işyeri bilgisi Kisiler veya aktif CokluSicilBaglantilari üzerinden çözülür; kullanıcı yetkisi dışındaki kayıtlar listelenmez.</summary>
        public DataTable GetVeriGirisleriAyIcin(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler)
        {
            string yetkilerValues = string.Join(",", yetkiler.Select(y =>
                $"({y.FirmaId},{(y.IsyeriId.HasValue ? y.IsyeriId.Value.ToString() : "NULL")})"
            ));

            var sql = $@"
DECLARE @Yil int = @p0;
DECLARE @Ay int = @p1;
DECLARE @AyBas date = DATEFROMPARTS(@Yil, @Ay, 1);
DECLARE @AySon date = EOMONTH(@AyBas);

-- Toplu veri girişi ekranı: kullanıcı yetkisi dışındaki final satırlar gösterilmez
DECLARE @Yetkiler TABLE (FirmaId INT, IsyeriId INT NULL);
INSERT INTO @Yetkiler (FirmaId, IsyeriId)
VALUES {yetkilerValues};

;WITH Kaynaksiz AS (
    SELECT
        f.SicilNo,
        f.Ad,
        f.Soyad,
        f.Tarih,
        f.CalismaTipi,
        f.Saat
    FROM dbo.FinalPuantajVerisi f
    WHERE f.Tarih >= @AyBas AND f.Tarih <= @AySon  -- Yalnızca seçilen ay
),
-- Çoklu sicil hedefinde firma/işyeri Kisiler yerine CokluSicilBaglantilari'ndan gelir
KJoin AS (
    SELECT
        x.SicilNo,
        x.Ad,
        x.Soyad,
        x.Tarih,
        x.CalismaTipi,
        x.Saat,
        COALESCE(k.FirmaId, c.FirmaId) AS FirmaId,
        COALESCE(k.IsyeriId, c.SirketId) AS IsyeriId
    FROM Kaynaksiz x
    LEFT JOIN dbo.Kisiler k ON k.PersonelId = x.SicilNo
    LEFT JOIN dbo.CokluSicilBaglantilari c 
           ON c.HedefPersonelId = x.SicilNo AND c.AktifMi = 1
)
SELECT
    SicilNo,
    Ad,
    Soyad,
    Tarih,
    CalismaTipi,
    Saat
FROM KJoin
WHERE EXISTS (
    SELECT 1 FROM @Yetkiler y 
    WHERE y.FirmaId = KJoin.FirmaId 
      AND (y.IsyeriId IS NULL OR y.IsyeriId = KJoin.IsyeriId)  -- Firma/İşyeri yetki filtresi
)
ORDER BY SicilNo, Tarih";

            var rows = _context.Database
                .SqlQueryRaw<VeriGirisRow>(sql,
                    new Microsoft.Data.SqlClient.SqlParameter("@p0", yil),
                    new Microsoft.Data.SqlClient.SqlParameter("@p1", ay))
                .ToList();

            var dt = new DataTable();
            dt.Columns.Add("SicilNo", typeof(int));
            dt.Columns.Add("Ad", typeof(string));
            dt.Columns.Add("Soyad", typeof(string));
            dt.Columns.Add("Tarih", typeof(DateTime));
            dt.Columns.Add("CalismaTipi", typeof(string));
            dt.Columns.Add("Saat", typeof(decimal));

            foreach (var r in rows)
            {
                dt.Rows.Add(r.SicilNo, r.Ad, r.Soyad, r.Tarih, r.CalismaTipi, r.Saat);
            }

            return dt;
        }

        /// <inheritdoc />
        public PuantajGunTipSaatDTO GetGunTipSaat(
            int personelId,
            DateTime tarih,
            DateTime? girisSaat = null,
            DateTime? cikisSaat = null,
            bool girisAcik = true,
            bool cikisAcik = true)
        {
            var sql = "EXEC dbo.sp_PuantajGunTipSaat @p0, @p1, @p2, @p3, @p4, @p5";
            var rows = _context.Database
                .SqlQueryRaw<PuantajGunTipSaatDTO>(sql,
                    new SqlParameter("@p0", SqlDbType.Int) { Value = personelId },
                    new SqlParameter("@p1", SqlDbType.Date) { Value = tarih.Date },
                    new SqlParameter("@p2", SqlDbType.DateTime) { Value = girisSaat.HasValue ? girisSaat.Value : DBNull.Value },
                    new SqlParameter("@p3", SqlDbType.DateTime) { Value = cikisSaat.HasValue ? cikisSaat.Value : DBNull.Value },
                    new SqlParameter("@p4", SqlDbType.Bit) { Value = girisAcik },
                    new SqlParameter("@p5", SqlDbType.Bit) { Value = cikisAcik })
                .ToList();

            return rows.FirstOrDefault() ?? new PuantajGunTipSaatDTO { CalismaTipi = "NG", Saat = 0 };
        }
    }
}
