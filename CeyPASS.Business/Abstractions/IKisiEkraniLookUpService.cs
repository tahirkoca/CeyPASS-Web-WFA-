using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IKisiEkraniLookUpService
    {
        List<LookupItem> GetCalismaStatuleri();
        List<LookupItem> GetDepartmanlar();
        List<LookupItem> GetPozisyonlar();
        List<LookupItem> GetIsyerleri(int firmId);
        List<LookupItem> GetFirma(int firmId);
        List<LookupItem> GetBolumler(int firmId);
    }
}
