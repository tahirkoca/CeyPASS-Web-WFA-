using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IKullaniciService
    {
        Kullanici GirisYap(string kullaniciAdi, string sifre);
        List<string> GetTumKullaniciAdlari();
    }
}
