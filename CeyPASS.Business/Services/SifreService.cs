using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;

namespace CeyPASS.Business.Services
{
    public class SifreService:ISifreService
    {
        private readonly IKullaniciRepository _repo;
        private readonly IEmailService _emailService;
        private readonly IKisiRepository _kisiRepo;
        private readonly IPersonelWebSifreRepository _personelSifreRepo;
        private readonly IBildirimService _bildirimService;
        private readonly IUstYetkiliRepository _ustYetkiliRepo;

        public SifreService(
            IKullaniciRepository repo, 
            IEmailService emailService,
            IKisiRepository kisiRepo,
            IPersonelWebSifreRepository personelSifreRepo,
            IBildirimService bildirimService,
            IUstYetkiliRepository ustYetkiliRepo)
        {
            _repo = repo;
            _emailService = emailService;
            _kisiRepo = kisiRepo;
            _personelSifreRepo = personelSifreRepo;
            _bildirimService = bildirimService;
            _ustYetkiliRepo = ustYetkiliRepo;
        }
        public string KodGonder(string kullaniciAdi)
        {
            return _repo.KullaniciyaKodGonder(kullaniciAdi);
        }
        public SifreSifirlamaSureci SifreSifirlamaBaslat(string kullaniciAdi)
        {
            var sonuc = new SifreSifirlamaSureci();
            kullaniciAdi = (kullaniciAdi ?? "").Trim();

            // 1. Önce doğrudan kurumsal kullanıcı adı ile ara
            var kullanici = _repo.GetByUserName(kullaniciAdi);
            
            // 2. Eğer bulunamadıysa, identifier (TC, Telefon, Email vb.) üzerinden Kisi'yi bul
            Kisi? kisi = null;
            if (kullanici == null)
            {
                kisi = _kisiRepo.GetByLoginIdentifier(kullaniciAdi);
                if (kisi != null)
                {
                    // Bu kişinin bir kurumsal hesabı var mı?
                    kullanici = _repo.GetByPersonelId(kisi.PersonelId);
                }
            }

            // 3. Kurumsal hesap bulunduysa (doğrudan veya Kisi üzerinden)
            if (kullanici != null)
            {
                if (string.IsNullOrWhiteSpace(kullanici.Email))
                {
                    sonuc.Basarili = false;
                    sonuc.HataMesaji = "Kurumsal hesabınıza ait e-posta bulunamadı.";
                    return sonuc;
                }

                var kod = new Random().Next(100000, 999999).ToString();
                var sonKullanmaZamani = DateTime.Now.AddMinutes(10);

                _repo.KurtarmaKoduKaydet(kullanici.KullaniciId, kod, sonKullanmaZamani);
                _emailService.SendVerificationCode(kullanici.Email, kod);

                sonuc.Basarili = true;
                sonuc.Email = kullanici.Email;
                return sonuc;
            }

            // 4. Kurumsal hesap yok ama Kisi kaydı varsa (Personel Portalı)
            if (kisi != null)
            {
                if (string.IsNullOrWhiteSpace(kisi.Email))
                {
                    // Amire bildirim gönder
                    var ustYetkiliId = _ustYetkiliRepo.GetUstYetkili(kisi.PersonelId);
                    if (!string.IsNullOrWhiteSpace(ustYetkiliId))
                    {
                        _bildirimService.AddNotification(
                            null,
                            ustYetkiliId,
                            "Şifre Sıfırlama Talebi",
                            $"{kisi.Ad} {kisi.Soyad} ({kisi.PersonelId}) şifresini unuttu ve e-postası tanımlı değil. Manuel şifre sıfırlama bekleniyor.",
                            "SifreReset"
                        );
                    }
                    else
                    {
                        // Üst yetkili yoksa tüm sistem adminlerine bildirim gönder
                        var adminIds = _repo.GetAdminUserIds();
                        foreach (var adminId in adminIds)
                        {
                            if (!string.IsNullOrWhiteSpace(adminId))
                            {
                                _bildirimService.AddNotification(
                                    null,
                                    adminId,
                                    "Şifre Sıfırlama Talebi (Üst Yetkilisiz)",
                                    $"{kisi.Ad} {kisi.Soyad} ({kisi.PersonelId}) şifresini unuttu. Üst yetkilisi tanımlı değil ve e-postası yok. İK tarafı kontrol etmeli.",
                                    "SifreReset"
                                );
                            }
                        }
                    }

                    sonuc.Basarili = false;
                    sonuc.HataMesaji = "NO_EMAIL|E-posta adresiniz sistemde kayıtlı görünmüyor. Lütfen şifrenizi sıfırlatmak için birim amirinizle veya İK ile iletişime geçiniz.";
                    return sonuc;
                }

                var kod = new Random().Next(100000, 999999).ToString();
                var sonKullanmaZamani = DateTime.Now.AddMinutes(10);

                _personelSifreRepo.KurtarmaKoduKaydet(kisi.PersonelId, kod, sonKullanmaZamani);
                _emailService.SendVerificationCode(kisi.Email, kod);

                sonuc.Basarili = true;
                sonuc.Email = kisi.Email;
                return sonuc;
            }

            sonuc.Basarili = false;
            sonuc.HataMesaji = "Girdiğiniz bilgilere ait bir kayıt bulunamadı.";
            return sonuc;
        }
        public SifreSifirlamaTamamlayici SifreSifirlamaTamamla(string kullaniciAdi, string girilenKod, string yeniSifre, string yeniSifreTekrar)
        {
            var sonuc = new SifreSifirlamaTamamlayici();
            if (!SifirlamaValidasyonYap(girilenKod, yeniSifre, yeniSifreTekrar, out string hata))
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = hata;
                return sonuc;
            }

            kullaniciAdi = (kullaniciAdi ?? "").Trim();

            // 1. Kurumsal Kullanıcı Kontrolü (Username ile)
            var kullanici = _repo.GetByUserName(kullaniciAdi);
            if (kullanici == null)
            {
                // Alternatif ID ile Kisi bul, oradan kurumsal hesaba git
                var kisiForCorp = _kisiRepo.GetByLoginIdentifier(kullaniciAdi);
                if (kisiForCorp != null)
                {
                    kullanici = _repo.GetByPersonelId(kisiForCorp.PersonelId);
                }
            }

            if (kullanici != null)
            {
                var kayitliKod = _repo.GetKurtarmaKodu(kullanici.KullaniciId);
                if (string.Equals(kayitliKod, girilenKod, StringComparison.Ordinal))
                {
                    if (_repo.SifreGuncelle(kullanici.KullaniciAdi, yeniSifre))
                    {
                        _repo.KurtarmaKodunuTemizle(kullanici.KullaniciId);
                        sonuc.Basarili = true;
                        return sonuc;
                    }
                }
            }

            // 2. Personel Kontrolü (Personel Portalı)
            var kisi = _kisiRepo.GetByLoginIdentifier(kullaniciAdi);
            if (kisi != null)
            {
                var kayitliKod = _personelSifreRepo.GetKurtarmaKodu(kisi.PersonelId);
                if (string.Equals(kayitliKod, girilenKod, StringComparison.Ordinal))
                {
                    if (_personelSifreRepo.EkleVeyaGuncelle(kisi.PersonelId, yeniSifre))
                    {
                        _personelSifreRepo.KurtarmaKodunuTemizle(kisi.PersonelId);
                        sonuc.Basarili = true;
                        return sonuc;
                    }
                }
            }

            sonuc.Basarili = false;
            sonuc.HataMesaji = "Doğrulama kodu hatalı veya kullanıcı bulunamadı.";
            return sonuc;
        }

        private bool SifirlamaValidasyonYap(string kod, string sifre, string tekrar, out string hata)
        {
            hata = "";
            if (string.IsNullOrWhiteSpace(kod)) { hata = "Doğrulama kodunu giriniz."; return false; }
            if (string.IsNullOrWhiteSpace(sifre)) { hata = "Yeni şifre giriniz."; return false; }
            if (sifre.Length < 6) { hata = "Şifre en az 6 karakter olmalıdır."; return false; }
            if (sifre != tekrar) { hata = "Şifreler uyuşmuyor."; return false; }
            return true;
        }

        public bool SifreSifirlaManuel(string personelId, string yeniSifre)
        {
            if (string.IsNullOrWhiteSpace(personelId) || string.IsNullOrWhiteSpace(yeniSifre)) return false;
            return _personelSifreRepo.EkleVeyaGuncelle(personelId, yeniSifre);
        }

        public bool SifreyiGuncelle(string kullaniciAdi, string yeniSifre, bool isCorporate = true)
        {
            if (isCorporate)
            {
                return _repo.SifreGuncelle(kullaniciAdi, yeniSifre);
            }
            else
            {
                // Personnel password update (WebSifreler)
                return _personelSifreRepo.EkleVeyaGuncelle(kullaniciAdi, yeniSifre);
            }
        }
    }
}
