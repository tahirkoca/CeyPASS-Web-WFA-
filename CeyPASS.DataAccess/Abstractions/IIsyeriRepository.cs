using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IIsyeriRepository
    {
        List<LookupItem> GetByFirma(int firmId);
        List<IsyeriItem> GetIsyerleriByFirma(int firmaId);
        DataTable GetAll();
        bool InsertManual(int firmaId, int isyeriId, string isyeriAdi);
        bool Update(int firmaId, int isyeriId, string isyeriAdi);
        bool Delete(int firmaId, int isyeriId);
    }
}
