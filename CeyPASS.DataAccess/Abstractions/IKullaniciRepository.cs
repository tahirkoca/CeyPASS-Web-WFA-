using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Sistem kullanıcıları, giriş ve şifre işlemleri.</summary>
    public interface IKullaniciRepository
    {
        /// <summary>Kullanici Dogrula işlemini gerçekleştirir.</summary>
        Kullanici KullaniciDogrula(string kullaniciAdi, string sifre);
        /// <summary>Sifre Guncelle işlemini gerçekleştirir.</summary>
        bool SifreGuncelle(string kullaniciAdi, string yeniSifre);
        /// <summary>Kullaniciya Kod Gonder işlemini gerçekleştirir.</summary>
        string KullaniciyaKodGonder(string kullaniciAdi);
        /// <summary>Isyeri Id List By Firma sorgularını getirir.</summary>
        List<int> GetIsyeriIdListByFirma(int firmaId);
        /// <summary>By User Name sorgularını getirir.</summary>
        Kullanici GetByUserName(string kullaniciAdi);
        /// <summary>By Personel Id sorgularını getirir.</summary>
        Kullanici GetByPersonelId(string personelId);
        /// <summary>Kurtarma Kodu Kaydet işlemini gerçekleştirir.</summary>
        void KurtarmaKoduKaydet(int kullaniciId, string kod, DateTime sonKullanmaZamani);
        /// <summary>Kurtarma Kodu sorgularını getirir.</summary>
        string GetKurtarmaKodu(int kullaniciId);
        /// <summary>Kurtarma Kodunu Temizle işlemini gerçekleştirir.</summary>
        void KurtarmaKodunuTemizle(int kullaniciId);
        /// <summary>Giriş ekranı dropdown için tüm kullanıcı adlarını getirir (yetkili tablosu).</summary>
        List<string> GetTumKullaniciAdlari();
        /// <summary>Admin User Ids sorgularını getirir.</summary>
        List<string> GetAdminUserIds();
    }
}
