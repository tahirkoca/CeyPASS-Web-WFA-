using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Canlı izleme kart komut kuyruğu işlemleri.</summary>
    public interface ICanliIzlemeKartKomutRepository
    {
        /// <summary>Enqueue işlemini gerçekleştirir.</summary>
        void Enqueue(int firmaId, string personelId, string kartNo, string komut, int? olusturanKullaniciId);
        /// <summary>Son Komut sorgularını getirir.</summary>
        string GetSonKomut(int firmaId, string personelId);
        /// <summary>Son Komut Map sorgularını getirir.</summary>
        IReadOnlyDictionary<string, string> GetSonKomutMap(int firmaId, IEnumerable<string> personelIds);
        /// <summary>Kart No By Personel Id sorgularını getirir.</summary>
        string GetKartNoByPersonelId(string personelId);
    }
}
