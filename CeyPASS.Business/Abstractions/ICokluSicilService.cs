using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Aynı TC ile birden fazla sicil bağlantısı yönetimi.</summary>
    public interface ICokluSicilService
    {
        /// <summary>Ana personele bağlı hedef siciller.</summary>
        List<CokluSicilBaglantiDTO> GetByAnaPersonelId(int anaPersonelId, bool yalnizcaAktif = false);

        /// <summary>TC ile eşleşebilecek hedef adayları.</summary>
        List<CokluSicilHedefAdayDTO> GetHedefAdaylari(int anaPersonelId, string tcKimlikNo);

        /// <summary>Personelin ana/hedef sicil özet bilgisi.</summary>
        CokluSicilOzetDTO GetOzet(int personelId);

        /// <summary>Bağlantıları oluşturur veya günceller.</summary>
        void Upsert(int anaPersonelId, string tcKimlikNo, CokluSicilUpsertRequest request, int? kullaniciId);

        /// <summary>Hedef sicil bağlantısını aktif/pasif yapar.</summary>
        void SetAktif(int anaPersonelId, int hedefPersonelId, bool aktif, int? kullaniciId);

        /// <summary>Ana sicile bağlı tüm hedefleri pasifleştirir.</summary>
        void PasifleştirTümünü(int anaPersonelId, int? kullaniciId);

        /// <summary>Personel ana sicil mi.</summary>
        bool IsAnaSicil(int personelId);

        /// <summary>Personel hedef sicil mi.</summary>
        bool IsHedefSicil(int personelId);

        /// <summary>Aktif hedef sicil sayısı.</summary>
        int GetAktifHedefSayisi(int personelId);
    }
}
