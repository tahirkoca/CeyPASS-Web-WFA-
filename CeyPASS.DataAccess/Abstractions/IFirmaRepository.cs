using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IFirmaRepository
    {
        DataTable GetFirmalar();
        List<Firma> GetPuantajFirmalari();
        List<LookupItem> GetSingle(int firmId);
        List<Firma> GetAll();
        bool Insert(Firma f);
        bool Update(Firma f);
        bool Delete(int id);
        int? GetMaxId();
    }
}
