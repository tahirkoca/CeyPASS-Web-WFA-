using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Firma tanım ve lookup işlemleri.</summary>
    public interface IFirmaRepository
    {
        /// <summary>Firmalar sorgularını getirir.</summary>
        DataTable GetFirmalar();
        /// <summary>Puantaj Firmalari sorgularını getirir.</summary>
        List<Firma> GetPuantajFirmalari();
        /// <summary>Single sorgularını getirir.</summary>
        List<LookupItem> GetSingle(int firmId);
        /// <summary>All sorgularını getirir.</summary>
        List<Firma> GetAll();
        /// <summary>Yeni kayıt ekler.</summary>
        bool Insert(Firma f);
        /// <summary>Kaydı günceller.</summary>
        bool Update(Firma f);
        /// <summary>Kaydı siler.</summary>
        bool Delete(int id);
        /// <summary>Max Id sorgularını getirir.</summary>
        int? GetMaxId();
    }
}
