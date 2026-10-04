using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Cihazda kart aktif/pasif komut kuyruğu.</summary>
    public interface ICanliIzlemeKartKomutService
    {
        /// <summary>Son komut PASIF değilse true (pasif kanıtı yok = aktif).</summary>
        bool IsKartCihazdaAktif(int firmaId, string personelId);

        /// <summary>PersonelId → cihaz aktif mi (pasif kanıtı yok = true).</summary>
        IReadOnlyDictionary<string, bool> GetCihazdaAktifMap(int firmaId, IEnumerable<string> personelIds);

        /// <summary>Cihazda kartı aktif etmek için komut kuyruğa alır.</summary>
        void EnqueueAktif(int firmaId, string personelId, int? olusturanKullaniciId = null);

        /// <summary>Cihazda kartı pasif etmek için komut kuyruğa alır. Açık ataması olmayan (HAZIR) kart kısıtlanamaz.</summary>
        void EnqueuePasif(int firmaId, string personelId, int? olusturanKullaniciId = null);

        /// <summary>Kart cihazda kısıtlıysa AKTIF komutu yazar.</summary>
        /// <returns>Komut yazıldıysa true.</returns>
        bool KisitVarsaKaldir(int firmaId, string personelId, int? olusturanKullaniciId = null);
    }
}
