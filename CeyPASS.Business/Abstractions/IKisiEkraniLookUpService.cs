using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IKisiEkraniLookUpService
    {
        List<LookupItem> GetCalismaStatuleri(int? firmId = null);
        List<LookupItem> GetDepartmanlar(int? firmId = null);
        List<LookupItem> GetPozisyonlar(int? firmId = null);
        List<LookupItem> GetIsyerleri(int firmId);
        List<LookupItem> GetFirma(int firmId);
        List<LookupItem> GetBolumler(int firmId);
        void InvalidateCache();
    }
}
