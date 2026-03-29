using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Abstractions
{
    public interface ISifreService
    {
        string KodGonder(string kullaniciAdi);
        bool SifreyiGuncelle(string kullaniciAdi, string yeniSifre, bool isCorporate = true);
        SifreSifirlamaSureci SifreSifirlamaBaslat(string kullaniciAdi);
        SifreSifirlamaTamamlayici SifreSifirlamaTamamla(string kullaniciAdi, string girilenKod, string yeniSifre, string yeniSifreTekrar);
        bool SifreSifirlaManuel(string personelId, string yeniSifre);
    }
}
