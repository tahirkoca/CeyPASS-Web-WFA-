using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Personel kayıt, çıkış ve puantaj listesi komutları.</summary>
    public interface IKisiService
    {
        /// <summary>Yeni personel ve ilişkili kart/yemek ayarları ile kayıt.</summary>
        void YeniKisiEkle(Kisi kisi, bool firmaPersoneli, bool puantajYapilabilir, bool yemekHakkiVar, int gunlukYemekLimiti, string puantajsizKartId, string puantajsizKartNo, string puantajsizKartAdi);

        /// <summary>Personeli işten çıkarır ve firma dışı kart atar.</summary>
        bool KisiIstenCikar(string personelId, DateTime cikisTarihi, string firmaDisiKartNo);

        /// <summary>Personel bilgilerini günceller.</summary>
        bool KisiGuncelle(Kisi kisi,string originalPersonelId,bool firmaPersoneli,bool puantajYapilabilir,bool yemekHakkiVar,int gunlukYemekAdedi,string firmaDisiKartNo,bool fotoDegisti);

        /// <summary>Puantaj ekranı için dönem personel listesi.</summary>
        List<Kisi> GetKisilerForPuantaj(int firmaId, int isyeriId, int yil, int ay);

        /// <summary>Ad soyad kısa bilgisi.</summary>
        KisiAdSoyad GetAdSoyad(string personelId);

        /// <summary>Kayıt öncesi doğrulama.</summary>
        (bool IsValid, string? Message) ValidateKisiKayit(KisiKayitValidasyonDTO dto);

        /// <summary>İşten çıkan personeli tekrar aktifleştirir.</summary>
        KisiTekrarAktifSonuc KisiTekrarAktifEt(string personelId, bool puantajYapilirMi);
    }
}
