using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Kullanıcı giriş ve temel kullanıcı sorguları.</summary>
    public interface IKullaniciService
    {
        /// <summary>Kullanıcı adı/şifre ile doğrulama.</summary>
        Kullanici GirisYap(string kullaniciAdi, string sifre);

        /// <summary>Personel kimliğine bağlı kullanıcı.</summary>
        Kullanici GetByPersonelId(string personelId);

        /// <summary>Kullanıcı adına göre kayıt.</summary>
        Kullanici GetByUserName(string kullaniciAdi);

        /// <summary>Sistemdeki tüm kullanıcı adları.</summary>
        List<string> GetTumKullaniciAdlari();
    }
}
