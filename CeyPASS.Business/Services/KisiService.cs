using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Business.Services
{
    public class KisiService : IKisiService
    {
        private readonly IKisiRepository _kisiRepo;
        private readonly IYemekhaneRepository _yemekhaneRepo;
        private readonly ILogger<KisiService>? _logger;

        public KisiService(
            IKisiRepository kisiRepo,
            IYemekhaneRepository yemekhaneRepo,
            ILogger<KisiService>? logger = null)
        {
            _kisiRepo = kisiRepo;
            _yemekhaneRepo = yemekhaneRepo;
            _logger = logger;
        }

        public void YeniKisiEkle(Kisi kisi, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekLimiti, string puantajsizKartId, string puantajsizKartNo, string puantajsizKartAdi)
        {
            kisi.PuantajYapilirMi = puantajYapilabilir;
            _kisiRepo.Insert(kisi, puantajsizKartNo);
            if (yemekHakkiVar)
                _yemekhaneRepo.InsertLimit(kisi.PersonelId, gunlukYemekLimiti);
        }

        public bool KisiIstenCikar(string personelId, DateTime cikisTarihi, string firmaDisiKartNo)
        {
            try
            {
                _kisiRepo.SetIstenCikisTarihi(personelId, cikisTarihi);
                _yemekhaneRepo.PasifEtByPersonel(personelId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool KisiGuncelle(Kisi kisi, string originalPersonelId, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekAdedi, string firmaDisiKartNo, bool fotoDegisti)
        {
            try
            {
                var ok = _kisiRepo.Update(kisi, originalPersonelId, fotoDegisti, firmaDisiKartNo);
                if (!ok) return false;

                if (yemekHakkiVar && gunlukYemekAdedi > 0)
                    _yemekhaneRepo.UpsertLimit(kisi.PersonelId, gunlukYemekAdedi);
                else
                    _yemekhaneRepo.PasifEtByPersonel(kisi.PersonelId);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "KisiGuncelle başarısız. PersonelId={PersonelId}, OriginalPersonelId={OriginalPersonelId}",
                    kisi?.PersonelId, originalPersonelId);
                return false;
            }
        }

        public List<Kisi> GetKisilerForPuantaj(int firmaId, int isyeriId, int yil, int ay)
        {
            return _kisiRepo.GetKisilerForPuantaj(firmaId, isyeriId, yil, ay);
        }

        public KisiAdSoyad GetAdSoyad(string personelId)
        {
            return _kisiRepo.GetAdSoyadByPersonelId(personelId);
        }

        public (bool IsValid, string? Message) ValidateKisiKayit(KisiKayitValidasyonDTO dto)
        {
            if (dto == null)
                return (false, "Kayıt bilgisi eksik.");

            bool firma = dto.FirmaPersoneli;
            bool puantaj = dto.PuantajYapilir;
            bool yemek = dto.YemekHakkiVar;
            bool taseron = dto.TaseronCalisanMi;
            bool ziyaretci = dto.ZiyaretciMi;
            bool arac = dto.AracKartiMi;

            var personelId = (dto.PersonelId ?? "").Trim();
            var tc = (dto.TcKimlikNo ?? "").Trim();
            var kartNo = (dto.KartNo ?? "").Trim();

            if (string.IsNullOrWhiteSpace(personelId))
                return (false, "PersonelId (Sicil No) giriniz.");

            bool puantajsizKartGerekli = (firma && !puantaj) || (!firma && !puantaj && yemek);
            if (puantajsizKartGerekli)
            {
                if (string.IsNullOrWhiteSpace(dto.FirmaDisiKartNo))
                    return (false, "Firma Dışı Kart No giriniz.");
            }

            if (yemek && dto.YemekAdedi <= 0)
                return (false, "Yemek hakkı var; günlük yemek adedini giriniz.");

            if (!firma && puantaj)
                return (false, "Bu check kombinasyonu (Firma personeli değil + Puantaj yapılabilir) için kural tanımlı değil.");

            bool firmaVeyaTaseron = firma || taseron;
            if (firmaVeyaTaseron)
            {
                if (string.IsNullOrWhiteSpace(tc))
                    return (false, "T.C. Kimlik No giriniz.");
            }

            if (ziyaretci && string.IsNullOrWhiteSpace(kartNo))
                return (false, "Kart No giriniz.");

            if (arac && string.IsNullOrWhiteSpace(kartNo))
                return (false, "Kart No giriniz.");

            if (!string.IsNullOrWhiteSpace(tc) && !IsValidTcKimlikNo(tc))
                return (false, "T.C. Kimlik No 11 haneli olmalıdır.");

            var sicilHit = _kisiRepo.FindByPersonelId(personelId);
            if (sicilHit != null)
                return (false, FormatCakisma("Sicil No", sicilHit));

            bool checkTc = firmaVeyaTaseron;
            bool checkKart = ziyaretci || arac || (firmaVeyaTaseron && !string.IsNullOrWhiteSpace(kartNo));

            if (checkTc)
            {
                var tcHit = _kisiRepo.FindByTcKimlikNo(tc);
                if (tcHit != null)
                    return (false, FormatCakisma("T.C. Kimlik No", tcHit));
            }

            if (checkKart)
            {
                var kartHit = _kisiRepo.FindByKartNo(kartNo);
                if (kartHit != null)
                    return (false, FormatCakisma("Kart No", kartHit));
            }

            return (true, null);
        }

        private static bool IsValidTcKimlikNo(string tc)
        {
            return tc.Length == 11 && tc.All(char.IsDigit);
        }

        private static string FormatCakisma(string alan, KisiAdSoyad hit)
        {
            var adSoyad = ((hit.Ad ?? "") + " " + (hit.Soyad ?? "")).Trim();
            var sicil = hit.PersonelId ?? "";
            if (string.IsNullOrWhiteSpace(adSoyad))
                return $"Bu {alan} zaten kayıtlı: {sicil}";
            return $"Bu {alan} zaten kayıtlı: {sicil} - {adSoyad}";
        }

        public KisiTekrarAktifSonuc KisiTekrarAktifEt(string personelId, bool puantajYapilirMi)
        {
            if (string.IsNullOrWhiteSpace(personelId))
                return KisiTekrarAktifSonuc.Basarisiz();

            try
            {
                var detay = _kisiRepo.GetDetay(personelId.Trim());
                if (detay == null || !detay.IstenCikisTarihi.HasValue)
                    return KisiTekrarAktifSonuc.Basarisiz();

                var cihazUyarisiGoster = !string.IsNullOrWhiteSpace(detay.KartNo);

                if (!_kisiRepo.TekrarAktifEt(personelId.Trim(), puantajYapilirMi))
                    return KisiTekrarAktifSonuc.Basarisiz();

                int? yenidenAktifYemekLimiti = null;
                string warningMessage = null;
                try
                {
                    var sonLimit = _yemekhaneRepo.GetSonGunlukLimit(personelId.Trim());
                    if (sonLimit.HasValue && sonLimit.Value > 0)
                    {
                        _yemekhaneRepo.UpsertLimit(personelId.Trim(), sonLimit.Value);
                        yenidenAktifYemekLimiti = sonLimit.Value;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "KisiTekrarAktifEt yemek limiti geri yükleme başarısız. PersonelId={PersonelId}", personelId);
                    warningMessage = "Personel aktif edildi ancak yemek limiti otomatik aktifleştirilemedi.";
                }

                return KisiTekrarAktifSonuc.Basarili(yenidenAktifYemekLimiti, cihazUyarisiGoster, warningMessage);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "KisiTekrarAktifEt başarısız. PersonelId={PersonelId}", personelId);
                return KisiTekrarAktifSonuc.Basarisiz(ex.Message);
            }
        }
    }
}
