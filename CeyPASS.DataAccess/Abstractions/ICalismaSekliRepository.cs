using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Firma ve global çalışma şekli tanımları.</summary>
    public interface ICalismaSekliRepository
    {
        /// <summary>All sorgularını getirir.</summary>
        List<CalismaSekli> GetAll(int firmaId, bool includeGlobal = true);
        /// <summary>All For Admin sorgularını getirir.</summary>
        List<CalismaSekli> GetAllForAdmin();
        /// <summary>Yeni kayıt ekler.</summary>
        int Insert(CalismaSekli x);
        /// <summary>Kaydı günceller.</summary>
        bool Update(CalismaSekli x);
        /// <summary>Kaydı siler.</summary>
        bool Delete(int id, int firmaId);
    }
}
