using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Pozisyon (görev) tanım CRUD.</summary>
    public interface IPozisyonService
    {
        /// <summary>Lookup pozisyon listesi.</summary>
        List<LookupItem> GetAll();

        /// <summary>Yönetim grid listesi.</summary>
        List<PozisyonListDTO> GetListForAdmin();

        /// <summary>Düzenleme formu için alanlar.</summary>
        (int id, string ad, string ack)? GetForEdit(int id);

        /// <summary>Yeni pozisyon.</summary>
        bool Add(string ad, string aciklama);

        /// <summary>Pozisyon güncelleme.</summary>
        bool Update(int id, string ad, string aciklama);

        /// <summary>Pozisyon silme.</summary>
        bool Delete(int id);
    }
}
