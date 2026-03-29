using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IAdminKullaniciRepository
    {
        List<KullaniciAdminRow> GetAll();
        bool SetPersonelId(int kullaniciId, int? personelId);
    }
}

