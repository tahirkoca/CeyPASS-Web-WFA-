using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>İşyeri tanım CRUD.</summary>
    public interface IIsyeriService
    {
        /// <summary>Tüm işyerleri tablo görünümü.</summary>
        DataTable GetAll();

        /// <summary>Yönetim listesi.</summary>
        List<IsyeriItem> GetListForAdmin();

        /// <summary>Firmaya bağlı işyerleri.</summary>
        List<IsyeriItem> GetIsyerleriByFirma(int firmaId);

        /// <summary>Manuel kimlik ile işyeri ekler.</summary>
        bool AddManual(int firmaId, int isyeriId, string ad);

        /// <summary>İşyeri adını günceller.</summary>
        bool Update(int firmaId, int isyeriId, string ad);

        /// <summary>İşyerini siler.</summary>
        bool Delete(int firmaId, int isyeriId);
    }
}
