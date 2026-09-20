using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Aynı TC için çoklu sicil bağlantı yönetimi.</summary>
    public interface ICokluSicilRepository
    {
        /// <summary>By Ana Personel Id sorgularını getirir.</summary>
        List<CokluSicilBaglantiDTO> GetByAnaPersonelId(int anaPersonelId, bool yalnizcaAktif = false);
        /// <summary>By Hedef Personel Id sorgularını getirir.</summary>
        List<CokluSicilBaglantiDTO> GetByHedefPersonelId(int hedefPersonelId, bool yalnizcaAktif = false);
        /// <summary>Hedef Adaylari sorgularını getirir.</summary>
        List<CokluSicilHedefAdayDTO> GetHedefAdaylari(string tcKimlikNo, int anaPersonelId);
        /// <summary>Ozet sorgularını getirir.</summary>
        CokluSicilOzetDTO GetOzet(int personelId);
        /// <summary>Upsert işlemini gerçekleştirir.</summary>
        void Upsert(CokluSicilUpsertRequest request, int anaPersonelId, string tcKimlikNo, int? kullaniciId);
        /// <summary>Aktif değerini ayarlar.</summary>
        void SetAktif(int anaPersonelId, int hedefPersonelId, bool aktif, int? kullaniciId);
        /// <summary>Ana sicile bagli tum hedef baglantilari pasiflestirir.</summary>
        void PasifleştirTümünü(int anaPersonelId, int? kullaniciId);
        /// <summary>Sicilin aktif coklu sicil ana kaydi olup olmadigini kontrol eder.</summary>
        bool IsAnaSicil(int personelId);
        /// <summary>Is Hedef Sicil işlemini gerçekleştirir.</summary>
        bool IsHedefSicil(int personelId);
        /// <summary>Aktif Hedef Sayisi sorgularını getirir.</summary>
        int GetAktifHedefSayisi(int anaPersonelId);
    }
}
