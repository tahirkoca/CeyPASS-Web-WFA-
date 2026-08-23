using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IPersonelVardiyaYemekYetkiRepository
    {
        bool FirmaHasSaatPenceresiAktif(int firmaId);
        List<PersonelVardiyaYemekYetki> GetByCalismaSekliId(int calismaSekliId);
        bool ExistsForCihaz(int calismaSekliId, int cihazId, int? excludeId = null);
        int Insert(PersonelVardiyaYemekYetki item);
        bool Update(PersonelVardiyaYemekYetki item);
        bool Delete(int id);
    }
}
