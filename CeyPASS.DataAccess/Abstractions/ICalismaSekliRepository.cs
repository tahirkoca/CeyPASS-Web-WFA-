using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface ICalismaSekliRepository
    {
        List<CalismaSekli> GetAll(int firmaId, bool includeGlobal = true);
        List<CalismaSekli> GetAllForAdmin();
        int Insert(CalismaSekli x);
        bool Update(CalismaSekli x);
        bool Delete(int id, int firmaId);
    }
}
