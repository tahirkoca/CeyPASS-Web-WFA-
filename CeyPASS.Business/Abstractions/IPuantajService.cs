using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>
    /// Aylık puantaj görüntüleme, onay/red, Logo dışa aktarım ve çoklu sicil işlemleri.
    /// </summary>
    public interface IPuantajService
    {
        /// <summary>Personelin seçilen ay için günlük puantaj özet satırlarını döner.</summary>
        List<PuantajGunSatirDTO> GetAy(int personelId, int yil, int ay);

        /// <summary>Günü onaylar ve nihai puantaj kaydına yazar.</summary>
        void Onayla(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, string calismaTipi, decimal saat, int kullaniciId);

        /// <summary>Ay içinde bekleyen ve düzenlenebilir tüm günleri toplu onaylar.</summary>
        void TopluOnayla(int personelId, int yil, int ay, int kullaniciId);

        /// <summary>Hedef tarihe kadar (dahil) bekleyen ve düzenlenebilir günleri toplu onaylar.</summary>
        void TopluOnaylaKadar(int personelId, int yil, int ay, DateTime hedefGun, int kullaniciId);

        /// <summary>Günü reddeder.</summary>
        void Reddet(int personelId, DateTime tarih, string aciklama, int kullaniciId);

        /// <summary>Onay kaydını düzeltme durumunda günceller (FM ve açıklama).</summary>
        void Duzenle(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, int kullaniciId);

        /// <summary>Çalışma/puantaj tipi listesini döner.</summary>
        List<PuantajTipDTO> GetPuantajTipleri();

        /// <summary>Düzeltme ile birlikte onaylar ve nihai kayda yazar.</summary>
        void DuzenleOnayla(int personelId, DateTime tarih, int duzenlenmisFm, string aciklama, string calismaTipi, decimal saat, int? kullaniciId);

        /// <summary>Ana sicilden bağlı sicillere aylık puantaj aktarımı yapar.</summary>
        void CokluSicileAktar(int anaPersonelId, int yil, int ay, int? kullaniciId);

        /// <summary>Ana sicile bağlı hedef sicil sayısını döner.</summary>
        int GetHedefSicilSayisi(int anaSicilNo);

        /// <summary>Sicilin çoklu sicil ana kaydı olup olmadığını belirler.</summary>
        bool IsAnaSicil(int sicilNo);

        /// <summary>Geçen ay için ek veri girişi izni verilen gün sayısını okur.</summary>
        int GetEkKayitGun();

        /// <summary>Geçen ay ek kayıt günü ayarını günceller.</summary>
        void SetEkKayitGun(int gun, int uid);

        /// <summary>FM tipi kodları için 7,5 saat üzeri fazla mesai dakikasını hesaplar.</summary>
        int HesaplaFazlaMesaiDakika(string calismaTipiKod, decimal saat);

        /// <summary>Logo Excel formatına uygun aylık puantaj satırlarını hesaplar ve denkleştirir.</summary>
        List<PuantajExportDTO> PrepareMonthlyExport(PuantajExportRequest request);

        /// <summary>Rapor (R) günlerini Logo kurallarına göre rapor günü ve NG gün sayısına ayırır.</summary>
        RaporGunHesaplamaResult HesaplaRaporGunleri(List<DateTime> raporTarihleri);

        /// <summary>Tarihin onay/düzenleme penceresinde olup olmadığını kontrol eder.</summary>
        bool IsRowEditable(DateTime tarih, int ekKayitGun);

        /// <summary>FM1 satırı için toplam çalışma saati (7,5 + FM dakika).</summary>
        decimal HesaplaFM1CalismaSaati(int fazlaMesaiDakika);

        /// <summary>Kullanıcının firma/işyeri yetki listesini döner.</summary>
        List<FirmaIsyeriYetkiDTO> GetKullaniciFirmaIsyeriYetkileri(int kullaniciId);

        /// <summary>Yetkilere göre ay için sicil listesini döner.</summary>
        DataTable GetSiciller(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler);

        /// <summary>Yetkilere göre ay için puantaj veri girişlerini döner.</summary>
        DataTable GetVeriGirisleri(int yil, int ay, List<FirmaIsyeriYetkiDTO> yetkiler);

        /// <summary>Seçili firma/işyeri/ay için beklenen-girilen-eksik özetini döner.</summary>
        PuantajVeriDurumuDTO GetVeriDurumu(int firmaId, int? isyeriId, int yil, int ay, int? personelId = null);
    }
}
