using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IPozisyonService
    {
        List<LookupItem> GetAll();
        List<PozisyonListDTO> GetListForAdmin();
        (int id, string ad, string ack)? GetForEdit(int id);
        bool Add(string ad, string aciklama);
        bool Update(int id, string ad, string aciklama);
        bool Delete(int id);
    }
}
