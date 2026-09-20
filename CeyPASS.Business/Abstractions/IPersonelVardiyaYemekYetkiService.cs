using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Çalışma şekline göre yemek saat penceresi yetkileri.</summary>
    public interface IPersonelVardiyaYemekYetkiService
    {
        /// <summary>Firmada saat penceresi kuralı aktif mi.</summary>
        bool FirmaHasSaatPenceresiAktif(int firmaId);

        /// <summary>Çalışma şekline bağlı yemek yetki kayıtları.</summary>
        List<PersonelVardiyaYemekYetki> GetByCalismaSekliId(int calismaSekliId);

        /// <summary>Yeni yetki kaydı.</summary>
        (bool ok, string error) Add(PersonelVardiyaYemekYetki item);

        /// <summary>Yetki kaydı güncelleme.</summary>
        (bool ok, string error) Update(PersonelVardiyaYemekYetki item);

        /// <summary>Yetki kaydı silme.</summary>
        bool Delete(int id);
    }
}
