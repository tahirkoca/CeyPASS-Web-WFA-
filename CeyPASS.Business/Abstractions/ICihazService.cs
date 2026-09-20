using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Geçiş cihazı tanım ve durum yönetimi.</summary>
    public interface ICihazService
    {
        /// <summary>Cihaz listesi; isteğe bağlı firma filtresi.</summary>
        List<CihazListDTO> GetListe(bool sadeceAktif, int? firmaId = null);

        /// <summary>Tek cihaz kaydı.</summary>
        Cihaz Get(int id);

        /// <summary>Yeni cihaz ekler.</summary>
        int Ekle(Cihaz c);

        /// <summary>Cihaz bilgilerini günceller.</summary>
        void Guncelle(Cihaz c);

        /// <summary>Cihazı pasifleştirir.</summary>
        void PasifYap(int id);

        /// <summary>Cihazı tekrar aktifleştirir.</summary>
        void AktifYap(int id);

        /// <summary>Cihaz tipi lookup listesi.</summary>
        List<CihazTip> GetCihazTipleri();
    }
}
