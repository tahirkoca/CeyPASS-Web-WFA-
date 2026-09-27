using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>PDKS cihaz tanım ve listeleme işlemleri.</summary>
    public interface ICihazRepository
    {
        /// <summary>List sorgularını getirir.</summary>
        List<CihazListDTO> GetList(bool sadeceAktif, int? firmaId = null, bool sadeceYemekhane = false);
        /// <summary>Kimliğe göre kaydı getirir.</summary>
        Cihaz GetById(int id);
        /// <summary>Yeni kayıt ekler.</summary>
        int Insert(Cihaz c);
        /// <summary>Kaydı günceller.</summary>
        void Update(Cihaz c);
        /// <summary>Aktif değerini ayarlar.</summary>
        void SetAktif(int id, bool aktif);
        /// <summary>Tips sorgularını getirir.</summary>
        List<CihazTip> GetTips();
    }
}
