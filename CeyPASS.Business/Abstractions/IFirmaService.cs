using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Firma tanım ve puantaj firma listesi.</summary>
    public interface IFirmaService
    {
        /// <summary>Tüm firmalar.</summary>
        List<Firma> GetAll();

        /// <summary>Combo/lookup için firma listesi.</summary>
        List<LookupItem> GetLookup();

        /// <summary>Manuel ekleme için önerilen kimlik.</summary>
        int SuggestNextId();

        /// <summary>Yeni firma ekler.</summary>
        bool Add(int id, string ad, string itMail, out string msg);

        /// <summary>Firma bilgilerini günceller.</summary>
        bool Update(int id, string ad, string itMail, out string msg);

        /// <summary>Firmayı siler.</summary>
        bool Delete(int id);

        /// <summary>Puantaj modülünde kullanılan firmalar.</summary>
        List<Firma> GetPuantajFirmalar();
    }
}
