using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>İşyeri (şirket) tanım ve lookup işlemleri.</summary>
    public interface IIsyeriRepository
    {
        /// <summary>By Firma sorgularını getirir.</summary>
        List<LookupItem> GetByFirma(int firmId);
        /// <summary>Isyerleri By Firma sorgularını getirir.</summary>
        List<IsyeriItem> GetIsyerleriByFirma(int firmaId);
        /// <summary>All sorgularını getirir.</summary>
        DataTable GetAll();
        /// <summary>Insert Manual işlemini gerçekleştirir.</summary>
        bool InsertManual(int firmaId, int isyeriId, string isyeriAdi);
        /// <summary>Kaydı günceller.</summary>
        bool Update(int firmaId, int isyeriId, string isyeriAdi);
        /// <summary>Kaydı siler.</summary>
        bool Delete(int firmaId, int isyeriId);
    }
}
