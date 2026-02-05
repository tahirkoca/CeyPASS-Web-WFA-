using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface ICalismaSekliService
    {
        List<CalismaSekli> GetAll(int firmaId, bool includeGlobal = true);
        List<CalismaSekli> GetAllForAdmin();
        int Add(CalismaSekli x);
        bool Update(CalismaSekli x);
        bool Delete(int id, int firmaId);
    }
}
