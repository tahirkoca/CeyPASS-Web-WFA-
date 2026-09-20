using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Çalışma statüsü lookup yönetimi.</summary>
    public interface ICalismaStatuService
    {
        /// <summary>Tüm statüleri listeler.</summary>
        List<LookupItem> GetAll();

        /// <summary>Manuel ekleme için bir sonraki kimlik.</summary>
        int GetNextId();

        /// <summary>Belirtilen kimlik ile statü ekler.</summary>
        bool Add(int id, string ad);

        /// <summary>Otomatik kimlik ile statü ekler.</summary>
        bool AddAuto(string ad);

        /// <summary>Statü adını günceller.</summary>
        bool Update(int id, string ad);

        /// <summary>Statüyü siler.</summary>
        bool Delete(int id);
    }
}
