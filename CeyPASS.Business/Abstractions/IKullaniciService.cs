using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Abstractions
{
    public interface IKullaniciService
    {
        Kullanici GirisYap(string kullaniciAdi, string sifre);
    }
}
