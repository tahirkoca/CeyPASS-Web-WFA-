using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IPozisyonRepository
    {
        List<LookupItem> GetByFirma();
        List<LookupItem> GetAll();
        List<PozisyonListDTO> GetListForAdmin();
        DataRow GetById(int id);
        bool Insert(string ad, string aciklama);
        bool Update(int id, string ad, string aciklama);
        bool Delete(int id);
    }
}
