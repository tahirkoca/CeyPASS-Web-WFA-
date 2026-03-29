using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IKullaniciService
    {
        Kullanici GirisYap(string kullaniciAdi, string sifre);
        Kullanici GetByPersonelId(string personelId);
        Kullanici GetByUserName(string kullaniciAdi);
        List<string> GetTumKullaniciAdlari();
    }
}
