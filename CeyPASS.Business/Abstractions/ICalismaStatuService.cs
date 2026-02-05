using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface ICalismaStatuService
    {
        List<LookupItem> GetAll();
        int GetNextId();
        bool Add(int id, string ad);
        bool AddAuto(string ad);
        bool Update(int id, string ad);
        bool Delete(int id);
    }
}
