using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    public interface IDepartmanService
    {
        List<LookupItem> GetAll();
        List<DepartmanListDTO> GetListForAdmin();
        DataRow? GetRowById(int id);
        int GetNextId();
        bool Add(int id, string ad, string aciklama);
        bool Update(int id, string ad, string aciklama);
        bool Delete(int id);
    }
}
