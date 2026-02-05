using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IKullaniciRepository
    {
        Kullanici KullaniciDogrula(string kullaniciAdi, string sifre);
        bool SifreGuncelle(string kullaniciAdi, string yeniSifre);
        string KullaniciyaKodGonder(string kullaniciAdi);
        List<int> GetIsyeriIdListByFirma(int firmaId);
        Kullanici GetByUserName(string kullaniciAdi);
        void KurtarmaKoduKaydet(int kullaniciId, string kod, DateTime sonKullanmaZamani);
        string GetKurtarmaKodu(int kullaniciId);
        void KurtarmaKodunuTemizle(int kullaniciId);
    }
}
