using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Departman (bölüm) tanım CRUD.</summary>
    public interface IDepartmanService
    {
        /// <summary>Lookup listesi; isteğe bağlı firma filtresi.</summary>
        List<LookupItem> GetAll(int? firmId = null);

        /// <summary>Yönetim grid listesi.</summary>
        List<DepartmanListDTO> GetListForAdmin();

        /// <summary>Düzenleme için satır verisi.</summary>
        DataRow? GetRowById(int id);

        /// <summary>Manuel ekleme için sonraki kimlik.</summary>
        int GetNextId();

        /// <summary>Yeni departman ekler.</summary>
        bool Add(int id, string ad, string aciklama);

        /// <summary>Departman günceller.</summary>
        bool Update(int id, string ad, string aciklama);

        /// <summary>Departman siler.</summary>
        bool Delete(int id);
    }
}
