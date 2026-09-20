using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Çalışma şekli (vardiya) tanım CRUD.</summary>
    public interface ICalismaSekliService
    {
        /// <summary>Firmaya (ve isteğe bağlı global) ait çalışma şekilleri.</summary>
        List<CalismaSekli> GetAll(int firmaId, bool includeGlobal = true);

        /// <summary>Yönetim ekranı için tüm kayıtlar.</summary>
        List<CalismaSekli> GetAllForAdmin();

        /// <summary>Yeni çalışma şekli ekler; yeni kimlik döner.</summary>
        int Add(CalismaSekli x);

        /// <summary>Mevcut kaydı günceller.</summary>
        bool Update(CalismaSekli x);

        /// <summary>Firmaya bağlı kaydı siler.</summary>
        bool Delete(int id, int firmaId);
    }
}
