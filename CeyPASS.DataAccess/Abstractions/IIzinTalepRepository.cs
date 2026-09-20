using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>İzin talep kayıtları ve onay akışı verisi.</summary>
    public interface IIzinTalepRepository
    {
        /// <summary>Ekle işlemini gerçekleştirir.</summary>
        int Ekle(IzinTalep talep);
        /// <summary>Kimliğe göre kaydı getirir.</summary>
        IzinTalep? GetById(int talepId);
        /// <summary>By Sonuc Kisi Izin Id sorgularını getirir.</summary>
        IzinTalep? GetBySonucKisiIzinId(int kisiIzinId);
        /// <summary>By Personel sorgularını getirir.</summary>
        List<IzinTalep> GetByPersonel(string personelId);
        /// <summary>Ust Yetkili Bekleyenler sorgularını getirir.</summary>
        List<IzinTalep> GetUstYetkiliBekleyenler(string ustYetkiliPersonelId);
        /// <summary>Ik Bekleyenler sorgularını getirir.</summary>
        List<IzinTalep> GetIkBekleyenler();

        /// <summary>Ust Yetkili Guncelle işlemini gerçekleştirir.</summary>
        bool UstYetkiliGuncelle(int talepId, IzinOnayDurumu durum, string? aciklama);
        /// <summary>Ik Guncelle işlemini gerçekleştirir.</summary>
        bool IkGuncelle(int talepId, IzinOnayDurumu durum, int ikKullaniciId, string? aciklama);

        /// <summary>Sonuc Kisi Izin Id değerini ayarlar.</summary>
        bool SetSonucKisiIzinId(int talepId, int kisiIzinId);

        /// <summary>Donus Imzasina Ac işlemini gerçekleştirir.</summary>
        bool DonusImzasinaAc(int talepId, int ikKullaniciId);
        /// <summary>Kullanim Imza At işlemini gerçekleştirir.</summary>
        bool KullanimImzaAt(int talepId, int personelKullaniciId);
    }
}

