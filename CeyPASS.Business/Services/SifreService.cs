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

        public SifreService(IKullaniciRepository repo, IEmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }
        public string KodGonder(string kullaniciAdi)
        {
            return _repo.KullaniciyaKodGonder(kullaniciAdi);
        }
        public SifreSifirlamaSureci SifreSifirlamaBaslat(string kullaniciAdi)
        {
            var sonuc = new SifreSifirlamaSureci();

            var kullanici = _repo.GetByUserName(kullaniciAdi);
            if (kullanici == null)
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Bu kullanıcı adına ait kayıt bulunamadı.";
                return sonuc;
            }

            if (string.IsNullOrWhiteSpace(kullanici.Email))
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Kullanıcıya ait e-posta bulunamadı.";
                return sonuc;
            }

            var kod = new Random().Next(100000, 999999).ToString();

            var sonKullanmaZamani = DateTime.Now.AddMinutes(10);

            _repo.KurtarmaKoduKaydet(kullanici.KullaniciId, kod,sonKullanmaZamani);

            _emailService.SendVerificationCode(kullanici.Email, kod);

            sonuc.Basarili = true;
            sonuc.Email = kullanici.Email;
            return sonuc;
        }
        public SifreSifirlamaTamamlayici SifreSifirlamaTamamla(string kullaniciAdi, string girilenKod, string yeniSifre, string yeniSifreTekrar)
        {
            var sonuc = new SifreSifirlamaTamamlayici();

            if (string.IsNullOrWhiteSpace(girilenKod))
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Lütfen doğrulama kodunu giriniz.";
                return sonuc;
            }

            if (string.IsNullOrWhiteSpace(yeniSifre))
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Lütfen yeni şifrenizi giriniz.";
                return sonuc;
            }

            if (yeniSifre.Length < 6)
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Şifre en az 6 karakter olmalıdır.";
                return sonuc;
            }

            if (yeniSifre != yeniSifreTekrar)
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Şifreler uyuşmuyor.";
                return sonuc;
            }

            var kullanici = _repo.GetByUserName(kullaniciAdi);
            if (kullanici == null)
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Kullanıcı bulunamadı.";
                return sonuc;
            }

            var kayitliKod = _repo.GetKurtarmaKodu(kullanici.KullaniciId);
            if (!string.Equals(kayitliKod, girilenKod, StringComparison.Ordinal))
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Doğrulama kodu hatalı.";
                return sonuc;
            }

            var guncelleSonuc = SifreyiGuncelle(kullaniciAdi, yeniSifre);
            if (!guncelleSonuc)
            {
                sonuc.Basarili = false;
                sonuc.HataMesaji = "Şifre güncellenemedi. Lütfen daha sonra tekrar deneyin.";
                return sonuc;
            }

            _repo.KurtarmaKodunuTemizle(kullanici.KullaniciId);

            sonuc.Basarili = true;
            return sonuc;
        }
        public bool SifreyiGuncelle(string kullaniciAdi, string yeniSifre)
        {
            return _repo.SifreGuncelle(kullaniciAdi, yeniSifre);
        }
    }
}
