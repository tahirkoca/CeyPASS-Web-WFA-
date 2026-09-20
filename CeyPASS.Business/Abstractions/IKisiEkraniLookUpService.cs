using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Kişi ekranı combo/lookup verileri (önbellekli).</summary>
    public interface IKisiEkraniLookUpService
    {
        /// <summary>Çalışma statüleri lookup.</summary>
        List<LookupItem> GetCalismaStatuleri(int? firmId = null);

        /// <summary>Pozisyon lookup.</summary>
        List<LookupItem> GetPozisyonlar(int? firmId = null);

        /// <summary>Firmaya ait işyerleri.</summary>
        List<LookupItem> GetIsyerleri(int firmId);

        /// <summary>Tek firma lookup satırı.</summary>
        List<LookupItem> GetFirma(int firmId);

        /// <summary>Firmaya ait bölümler.</summary>
        List<LookupItem> GetBolumler(int firmId);

        /// <summary>Lookup önbelleğini temizler.</summary>
        void InvalidateCache();
    }
}
