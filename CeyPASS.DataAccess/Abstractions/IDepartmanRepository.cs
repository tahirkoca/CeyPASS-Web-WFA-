using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Departman tanım CRUD ve listeleme.</summary>
    public interface IDepartmanRepository
    {
        /// <summary>By Firma sorgularını getirir.</summary>
        List<LookupItem> GetByFirma(int? firmId = null);
        /// <summary>All sorgularını getirir.</summary>
        DataTable GetAll();
        /// <summary>Next Id sorgularını getirir.</summary>
        int GetNextId();
        /// <summary>Yeni kayıt ekler.</summary>
        bool Insert(int id, string ad, string aciklama);
        /// <summary>Kaydı günceller.</summary>
        bool Update(int id, string ad, string aciklama);
        /// <summary>Kaydı siler.</summary>
        bool Delete(int id);
    }
}
