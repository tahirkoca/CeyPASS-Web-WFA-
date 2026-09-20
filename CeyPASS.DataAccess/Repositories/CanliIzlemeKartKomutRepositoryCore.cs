using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.DataAccess.Repositories
{
    /// <summary>Kart komut kuyruğu erişimi.</summary>
    public class CanliIzlemeKartKomutRepositoryCore : ICanliIzlemeKartKomutRepository
    {
        private readonly CeyPASSDataConnectionCore _context;

        public CanliIzlemeKartKomutRepositoryCore(CeyPASSDataConnectionCore context)
        {
            _context = context;
        }

        /// <summary>Enqueue işlemini kuyruğa ekler.</summary>
        public void Enqueue(int firmaId, string personelId, string kartNo, string komut, int? olusturanKullaniciId)
        {
            _context.CanliIzlemeKartKomutKuyrugu.Add(new CanliIzlemeKartKomutKuyrugu
            {
                FirmaId = firmaId,
                PersonelId = (personelId ?? "").Trim(),
                KartNo = string.IsNullOrWhiteSpace(kartNo) ? null : kartNo.Trim(),
                Komut = (komut ?? "").Trim().ToUpperInvariant(),
                Tarih = DateTime.Now,
                OkunduMu = false,
                OlusturanKullaniciId = olusturanKullaniciId
            });
            _context.SaveChanges();
        }

        /// <summary>Son Komut sorgularını getirir.</summary>
        public string GetSonKomut(int firmaId, string personelId)
        {
            var pid = (personelId ?? "").Trim();
            if (string.IsNullOrEmpty(pid)) return null;

            return _context.CanliIzlemeKartKomutKuyrugu
                .Where(x => x.FirmaId == firmaId && x.PersonelId == pid)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Komut)
                .FirstOrDefault();
        }

        /// <summary>Son Komut Map sorgularını getirir.</summary>
        public IReadOnlyDictionary<string, string> GetSonKomutMap(int firmaId, IEnumerable<string> personelIds)
        {
            var ids = (personelIds ?? Enumerable.Empty<string>())
                .Select(x => (x ?? "").Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (ids.Count == 0)
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var rows = _context.CanliIzlemeKartKomutKuyrugu
                .Where(x => x.FirmaId == firmaId && ids.Contains(x.PersonelId))
                .Select(x => new { x.PersonelId, x.Id, x.Komut })
                .ToList();

            return rows
                .GroupBy(x => x.PersonelId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.Id).First().Komut ?? "",
                    StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Kart No By Personel Id sorgularını getirir.</summary>
        public string GetKartNoByPersonelId(string personelId)
        {
            var pid = (personelId ?? "").Trim();
            if (string.IsNullOrEmpty(pid)) return null;

            return _context.Kisiler
                .Where(k => k.PersonelId == pid)
                .Select(k => k.KartNo)
                .FirstOrDefault();
        }
    }
}
