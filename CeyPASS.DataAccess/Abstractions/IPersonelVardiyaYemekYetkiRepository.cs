using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Personel vardiya ve yemekhane yetki tanımları.</summary>
    public interface IPersonelVardiyaYemekYetkiRepository
    {
        /// <summary>Firma Has Saat Penceresi Aktif işlemini gerçekleştirir.</summary>
        bool FirmaHasSaatPenceresiAktif(int firmaId);
        /// <summary>By Calisma Sekli Id sorgularını getirir.</summary>
        List<PersonelVardiyaYemekYetki> GetByCalismaSekliId(int calismaSekliId);
        /// <summary>Exists For Cihaz işlemini gerçekleştirir.</summary>
        bool ExistsForCihaz(int calismaSekliId, int cihazId, int? excludeId = null);
        /// <summary>Yeni kayıt ekler.</summary>
        int Insert(PersonelVardiyaYemekYetki item);
        /// <summary>Kaydı günceller.</summary>
        bool Update(PersonelVardiyaYemekYetki item);
        /// <summary>Kaydı siler.</summary>
        bool Delete(int id);
    }
}
