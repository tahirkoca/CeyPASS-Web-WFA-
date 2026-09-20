using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Business.Services
{
    /// <summary>Kart aktif/pasif cihaz komut kuyruğu.</summary>
    public class CanliIzlemeKartKomutService : ICanliIzlemeKartKomutService
    {
        private readonly ICanliIzlemeKartKomutRepository _repo;

        public CanliIzlemeKartKomutService(ICanliIzlemeKartKomutRepository repo)
        {
            _repo = repo;
        }

        /// <inheritdoc />
        public bool IsKartCihazdaAktif(int firmaId, string personelId)
        {
            var son = _repo.GetSonKomut(firmaId, personelId);
            return !IsPasifKomut(son);
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, bool> GetCihazdaAktifMap(int firmaId, IEnumerable<string> personelIds)
        {
            var ids = (personelIds ?? Enumerable.Empty<string>())
                .Select(x => (x ?? "").Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sonMap = _repo.GetSonKomutMap(firmaId, ids);
            var result = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var id in ids)
            {
                sonMap.TryGetValue(id, out var son);
                result[id] = !IsPasifKomut(son);
            }
            return result;
        }

        /// <inheritdoc />
        public void EnqueueAktif(int firmaId, string personelId, int? olusturanKullaniciId = null)
            => Enqueue(firmaId, personelId, CanliIzlemeKartKomutTurleri.Aktif, olusturanKullaniciId);

        /// <inheritdoc />
        public void EnqueuePasif(int firmaId, string personelId, int? olusturanKullaniciId = null)
            => Enqueue(firmaId, personelId, CanliIzlemeKartKomutTurleri.Pasif, olusturanKullaniciId);

        private void Enqueue(int firmaId, string personelId, string komut, int? olusturanKullaniciId)
        {
            var pid = (personelId ?? "").Trim();
            if (string.IsNullOrEmpty(pid))
                throw new ArgumentException("PersonelId gerekli.", nameof(personelId));

            var kartNo = _repo.GetKartNoByPersonelId(pid);
            _repo.Enqueue(firmaId, pid, kartNo, komut, olusturanKullaniciId);
        }

        private static bool IsPasifKomut(string son)
            => !string.IsNullOrWhiteSpace(son)
               && string.Equals(son.Trim(), CanliIzlemeKartKomutTurleri.Pasif, StringComparison.OrdinalIgnoreCase);
    }
}
