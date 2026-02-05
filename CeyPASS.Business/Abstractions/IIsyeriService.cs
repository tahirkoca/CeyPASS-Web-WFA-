using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    public interface IIsyeriService
    {
        DataTable GetAll();
        List<IsyeriItem> GetListForAdmin();
        List<IsyeriItem> GetIsyerleriByFirma(int firmaId);
        bool AddManual(int firmaId, int isyeriId, string ad);
        bool Update(int firmaId, int isyeriId, string ad);
        bool Delete(int firmaId, int isyeriId);
    }
}
