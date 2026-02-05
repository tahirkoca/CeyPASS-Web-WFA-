using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IBolumRepository
    {
        List<LookupItem> GetByFirma(int firmaId);
    }
}
