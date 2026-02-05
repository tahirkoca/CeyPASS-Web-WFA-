using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IDepartmanRepository
    {
        List<LookupItem> GetByFirma();
        DataTable GetAll();
        int GetNextId();
        bool Insert(int id, string ad, string aciklama);
        bool Update(int id, string ad, string aciklama);
        bool Delete(int id);
    }
}
