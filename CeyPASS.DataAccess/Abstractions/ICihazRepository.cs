using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface ICihazRepository
    {
        List<CihazListDTO> GetList(bool sadeceAktif, int? firmaId = null);
        Cihaz GetById(int id);
        int Insert(Cihaz c);
        void Update(Cihaz c);
        void SetAktif(int id, bool aktif);
        List<CihazTip> GetTips();
    }
}
