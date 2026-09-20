using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Çalışma statüsü (personel durumu) tanımları.</summary>
    public interface ICalismaStatuRepository
    {
        /// <summary>By Firma sorgularını getirir.</summary>
        List<LookupItem> GetByFirma(int? firmId = null);
        /// <summary>Next Id sorgularını getirir.</summary>
        int GetNextId();
        /// <summary>Yeni kayıt ekler.</summary>
        bool Insert(int id, string ad);
        /// <summary>Kaydı günceller.</summary>
        bool Update(int id, string ad);
        /// <summary>Kaydı siler.</summary>
        bool Delete(int id);
    }
}
