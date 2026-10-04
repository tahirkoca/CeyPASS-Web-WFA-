using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Aylık puantaj, onay ve toplu veri girişi sorguları.</summary>
    public interface IPuantajRepository
    {
        /// <summary>sp_AylikPuantajVeri ile günlük puantaj özet satırlarını okur; onay bilgisi PuantajOnay ile birleştirilir.</summary>
        List<PuantajGunSatirDTO> SpPuantajAyOzet(int personelId, int yil, int ay);
        /// <summary>Günlük puantaj onay kaydını sp_Puantaj_Onay_Upsert ile yazar/günceller.</summary>
        void Sp_OnayUpsert(object con, int personelId, DateTime tarih, int onayDurumu, int duzenlenmisFm, string aciklama, int? kullaniciId);
        /// <summary>Final puantaj satırını sp_Puantaj_Final_Upsert ile yazar/günceller.</summary>
        void Sp_FinalUpsert(object con, int personelId, DateTime tarih, string calismaTipi, decimal saat, int? kullaniciId);
        /// <summary>Onay ve final puantaj yazımını tek transaction içinde uygular.</summary>
        void ApproveAndWriteFinal(int personelId, DateTime tarih, int onayDurumu, int duzenlenmisFm, string aciklama, string calismaTipi, decimal saat, int? kullaniciId);
        /// <summary>Yalnızca onay kaydını transaction ile yazar/günceller.</summary>
        void OnayUpsert(int personelId, DateTime tarih, int onayDurumu, int duzenlenmisFm, string aciklama, int? kullaniciId);
        /// <summary>Aktif puantaj tiplerini sp_PuantajTipleri_GetActive ile getirir.</summary>
        List<PuantajTipDTO> GetPuantajTipleri();
        /// <summary>Ana sicil puantajını bağlı hedef sicillere sp_CokluSicileAktar ile kopyalar.</summary>
        void CokluSicileAktar(int anaKey, int yil, int ay, int? kullaniciId);
        /// <summary>Ana sicile bağlı aktif hedef sicil sayısını döner.</summary>
        int GetHedefSicilSayisi(int anaSicilNo);
        /// <summary>Sicilin aktif çoklu sicil ana kaydı olup olmadığını kontrol eder.</summary>
        bool IsAnaSicil(int sicilNo);
        /// <summary>Sistem ayarı EkKayitGun değerini okur (geçmiş ay ek kayıt penceresi).</summary>
        int GetEkKayitGun();
        /// <summary>EkKayitGun sistem ayarını günceller.</summary>
        void SetEkKayitGun(int gun, int? kullaniciId);
        /// <summary>Kullanıcının firma/işyeri yetki listesini döner.</summary>
        List<FirmaIsyeriYetkiDTO> GetKullaniciFirmaIsyeriYetkileri(int kullaniciId);
        /// <summary>Seçilen ay için puantaj yapılacak sicil listesini döner. PuantajYapilirMi, işe giriş-çıkış ay penceresi, aktif CokluSicilBaglantilari (hedef siciller) ve kullanıcı Firma/İşyeri yetkileri birleştirilir; ana sicil çoklu bağlıysa BaseOnly dışlanır.</summary>
        DataTable GetSicillerAyIcin(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler);
        /// <summary>Ay içindeki FinalPuantajVerisi satırlarını yetkiye göre filtreler. Sicilin firma/işyeri bilgisi Kisiler veya aktif CokluSicilBaglantilari üzerinden çözülür; kullanıcı yetkisi dışındaki kayıtlar listelenmez.</summary>
        DataTable GetVeriGirisleriAyIcin(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler);

        /// <summary>sp_PuantajGunTipSaat — tek gün ÇalışmaTipi / Saat (override opsiyonel).</summary>
        PuantajGunTipSaatDTO GetGunTipSaat(
            int personelId,
            DateTime tarih,
            DateTime? girisSaat = null,
            DateTime? cikisSaat = null,
            bool girisAcik = true,
            bool cikisAcik = true);
    }
}
