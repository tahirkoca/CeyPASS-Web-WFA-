using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Şifre sıfırlama, doğrulama kodu ve güncelleme.</summary>
    public interface ISifreService
    {
        /// <summary>Doğrulama kodu üretir ve gönderir.</summary>
        string KodGonder(string kullaniciAdi);

        /// <summary>Şifreyi günceller (kurumsal politika bayrağı ile).</summary>
        bool SifreyiGuncelle(string kullaniciAdi, string yeniSifre, bool isCorporate = true);

        /// <summary>E-posta/SMS ile sıfırlama sürecini başlatır.</summary>
        SifreSifirlamaSureci SifreSifirlamaBaslat(string kullaniciAdi);

        /// <summary>Kod doğrulayıp şifreyi tamamlar.</summary>
        SifreSifirlamaTamamlayici SifreSifirlamaTamamla(string kullaniciAdi, string girilenKod, string yeniSifre, string yeniSifreTekrar);

        /// <summary>Yönetici tarafından manuel şifre sıfırlama.</summary>
        bool SifreSifirlaManuel(string personelId, string yeniSifre);
    }
}
