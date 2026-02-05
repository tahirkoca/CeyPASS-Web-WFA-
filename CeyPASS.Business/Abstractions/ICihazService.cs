using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface ICihazService
    {
        List<CihazListDTO> GetListe(bool sadeceAktif, int? firmaId = null);
        Cihaz Get(int id);
        int Ekle(Cihaz c);
        void Guncelle(Cihaz c);
        void PasifYap(int id);
        void AktifYap(int id);
        List<CihazTip> GetCihazTipleri();
    }
}
