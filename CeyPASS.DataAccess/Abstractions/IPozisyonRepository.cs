using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Pozisyon tanım işlemleri.</summary>
    public interface IPozisyonRepository
    {
        /// <summary>By Firma sorgularını getirir.</summary>
        List<LookupItem> GetByFirma(int? firmId = null);
        /// <summary>All sorgularını getirir.</summary>
        List<LookupItem> GetAll();
        /// <summary>List For Admin sorgularını getirir.</summary>
        List<PozisyonListDTO> GetListForAdmin();
        /// <summary>Kimliğe göre kaydı getirir.</summary>
        DataRow GetById(int id);
        /// <summary>Yeni kayıt ekler.</summary>
        bool Insert(string ad, string aciklama);
        /// <summary>Kaydı günceller.</summary>
        bool Update(int id, string ad, string aciklama);
        /// <summary>Kaydı siler.</summary>
        bool Delete(int id);
    }
}
