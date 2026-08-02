using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IPersonelVardiyaYemekYetkiService
    {
        bool FirmaHasSaatPenceresiAktif(int firmaId);
        List<PersonelVardiyaYemekYetki> GetByCalismaSekliId(int calismaSekliId);
        (bool ok, string error) Add(PersonelVardiyaYemekYetki item);
        (bool ok, string error) Update(PersonelVardiyaYemekYetki item);
        bool Delete(int id);
    }
}
