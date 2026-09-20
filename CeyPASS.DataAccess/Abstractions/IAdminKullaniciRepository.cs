using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Yönetici kullanıcı CRUD işlemleri.</summary>
    public interface IAdminKullaniciRepository
    {
        /// <summary>All sorgularını getirir.</summary>
        List<KullaniciAdminRow> GetAll();
        /// <summary>Personel Id değerini ayarlar.</summary>
        bool SetPersonelId(int kullaniciId, int? personelId);
    }
}

