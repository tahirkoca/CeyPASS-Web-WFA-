using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface ICalismaStatuRepository
    {
        List<LookupItem> GetByFirma();
        int GetNextId();
        bool Insert(int id, string ad);
        bool Update(int id, string ad);
        bool Delete(int id);
    }
}
