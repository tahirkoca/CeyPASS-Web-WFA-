using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CeyPASS.Business.Services
{
    public class MobileQrService : IMobileQrService
    {
        private readonly ICihazRepository _cihazRepository;
        private readonly IKisiRepository _kisiRepository;
        private readonly CeyPASSDataConnectionCore _context;

        public MobileQrService(
            ICihazRepository cihazRepository,
            IKisiRepository kisiRepository,
            CeyPASSDataConnectionCore context)
        {
            _cihazRepository = cihazRepository;
            _kisiRepository = kisiRepository;
            _context = context;
        }

        public ApiResult<string> ProcessQrScan(QrIstekModel request, string personelId)
        {
            try
            {
                // 1. Cihazı Bul ve Kontrol Et
                var cihaz = _cihazRepository.GetById(request.CihazId);
                if (cihaz == null || !cihaz.AktifMi)
                    return ApiResult<string>.Failure("Geçersiz veya pasif bir QR kod okuttunuz.");

                // 2. Güvenli Lokasyon Kontrolü
                if (request.IsMocked)
                    return ApiResult<string>.Failure("Güvenlik ihlali: Sahte konum (Fake GPS) kullanımı tespit edildi. İşlem reddedildi.");

                if (cihaz.Latitude.HasValue && cihaz.Longitude.HasValue)
                {
                    if (!request.Enlem.HasValue || !request.Boylam.HasValue)
                        return ApiResult<string>.Failure("Uygulamanın konum erişimi yok. Güvenlik kuralları gereği QR okutabilmek için cihaza yakın olduğunuzu konumunuzla doğrulamanız gerekir.");

                    double pEnlem = request.Enlem.Value;
                    double pBoylam = request.Boylam.Value;
                    double cEnlem = (double)cihaz.Latitude.Value;
                    double cBoylam = (double)cihaz.Longitude.Value;

                    double mesafeMetre = HesaplaMesafeVeMetreDon(cEnlem, cBoylam, pEnlem, pBoylam);
                    int tolerans = cihaz.MesafeToleransMetre ?? 50;

                    if (mesafeMetre > tolerans)
                        return ApiResult<string>.Failure($"Güvenlik ihlali: İşlem yapmak istediğiniz cihaza çok uzaksınız. Konumunuz cihaza {Math.Round(mesafeMetre)} metre uzaklıkta. Lütfen cihaza yaklaşın.");
                }

                // 3. Personeli Bul
                var personel = _kisiRepository.GetDetay(personelId);
                if (personel == null)
                    return ApiResult<string>.Failure("Personel kaydı bulunamadı.");

                // 4. Cihaz Tipini ve Hareket Tipini Dinamik Olarak Belirle
                var cihazTipEntity = _context.CihazTipler.FirstOrDefault(t => t.TipId == cihaz.CihazTipi);
                string hareketTipi = cihazTipEntity?.TipAdi ?? "Giriş";
                bool isYemekhane = hareketTipi.Contains("Yemekhane") || hareketTipi.Contains("Yemek");

                // MÜKERRER KAYIT KONTROLÜ: Son 5 saniye içinde aynı kayıt var mı?
                int pIdInt = 0;
                int.TryParse(personelId, out pIdInt);

                var sonKayit = _context.KisiHareketler
                    .Where(x => x.PersonelId == pIdInt && x.CihazId == request.CihazId)
                    .OrderByDescending(x => x.Tarih)
                    .FirstOrDefault();

                if (sonKayit != null && (DateTime.Now - sonKayit.Tarih).TotalSeconds < 5)
                    return ApiResult<string>.Ok("İşlem zaten başarılı (mükerrer kayıt engellendi).");

                // 5. KisiHareketler Tablosuna Logla
                var insertHareket = @"
                    INSERT INTO KisiHareketler (FirmaId, CihazId, PersonelId, Tarih, Tip, KayitZamani, AktifMi)
                    VALUES (@p0, @p1, @p2, GETDATE(), @p3, GETDATE(), 1)"; 
                _context.Database.ExecuteSqlRaw(insertHareket, 
                    new SqlParameter("@p0", cihaz.FirmaId),
                    new SqlParameter("@p1", cihaz.CihazId),
                    new SqlParameter("@p2", personelId),
                    new SqlParameter("@p3", hareketTipi));

                if (isYemekhane)
                {
                    // Bugünkü geçiş sayısını bul
                    var sayCmd = "SELECT COUNT(*) FROM YemekhaneGecisHareketler WHERE PersonelId = @p0 AND CAST(Tarih AS DATE) = CAST(GETDATE() AS DATE) AND AktifMi = 1";
                    int gecisSayisi = ExecuteScalarInt(sayCmd, personel.PersonelId.ToString());

                    // Günlük Limit
                    var limitCmd = "SELECT GunlukLimit FROM YemekhaneGirisLimitler WHERE PersonelId = @p0 AND AktifMi = 1";
                    int? gunlukLimit = ExecuteScalarNullableInt(limitCmd, personel.PersonelId.ToString());

                    if (gunlukLimit.HasValue && gecisSayisi >= gunlukLimit.Value)
                        return ApiResult<string>.Failure("Günlük yemekhane limitinizi doldurdunuz. İşlem reddedildi.");

                    // Yemekhane limit uygunsa, Yemekhane özel hareketlerine yaz.
                    var insertYemekhane = @"
                        INSERT INTO YemekhaneGecisHareketler (CihazId, PersonelId, Tarih, Saat, KayitZamani, AktifMi)
                        VALUES (@p0, @p1, CAST(GETDATE() AS DATE), CAST(CAST(GETDATE() AS TIME) AS DATETIME), GETDATE(), 1)";
                    _context.Database.ExecuteSqlRaw(insertYemekhane, new SqlParameter("@p0", cihaz.CihazId), new SqlParameter("@p1", personel.PersonelId));
                }

                // 6. Cihaza Aç komutunu CihazTetikKuyrugu'na yaz
                string logHareketi = "QR_" + hareketTipi.ToUpper().Replace("İ", "I").Replace("Ş", "S");
                var kuyrukCmd = "INSERT INTO CihazTetikKuyrugu (CihazId, PersonelId, Komut, Tarih, OkunduMu) VALUES (@p0, @p1, @p2, GETDATE(), 0)";
                _context.Database.ExecuteSqlRaw(kuyrukCmd, 
                    new SqlParameter("@p0", cihaz.CihazId),
                    new SqlParameter("@p1", personelId),
                    new SqlParameter("@p2", logHareketi));

                return ApiResult<string>.Ok("Giriş İşlemi Başlatıldı, Kapı Onayı Bekleniyor.");
            }
            catch (Exception ex)
            {
                return ApiResult<string>.Failure($"QR Okutma Hatası: {ex.Message}");
            }
        }

        private static double HesaplaMesafeVeMetreDon(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371e3; // Dünya yarıçapı (metre)
            var p1 = lat1 * Math.PI / 180;
            var p2 = lat2 * Math.PI / 180;
            var dp = (lat2 - lat1) * Math.PI / 180;
            var dl = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(dp / 2) * Math.Sin(dp / 2) +
                    Math.Cos(p1) * Math.Cos(p2) *
                    Math.Sin(dl / 2) * Math.Sin(dl / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return r * c; // Metre cinsinden mesafe
        }

        private int ExecuteScalarInt(string sql, string p0)
        {
            var value = ExecuteScalarNullableInt(sql, p0);
            return value ?? 0;
        }

        private int? ExecuteScalarNullableInt(string sql, string p0)
        {
            var connection = _context.Database.GetDbConnection();
            bool wasClosed = connection.State == System.Data.ConnectionState.Closed;

            if (wasClosed) connection.Open();

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = sql;

                var param = command.CreateParameter();
                param.ParameterName = "@p0";
                param.Value = p0;
                command.Parameters.Add(param);

                var result = command.ExecuteScalar();
                return result == DBNull.Value || result == null ? (int?)null : Convert.ToInt32(result);
            }
            finally
            {
                if (wasClosed) connection.Close();
            }
        }
    }
}
