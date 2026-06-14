using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Linq;

namespace CeyPASS.Business.Services
{
    public class KullaniciFirmaIsyeriYetkiService : IKullaniciFirmaIsyeriYetkiService
    {
        private readonly IKullaniciFirmaIsyeriYetkiRepository _repo;

        public KullaniciFirmaIsyeriYetkiService(IKullaniciFirmaIsyeriYetkiRepository repo)
        {
            _repo = repo;
        }

        public List<FirmaIsyeriYetkiDTO> GetYetkiler(int kullaniciId) => _repo.GetYetkiler(kullaniciId);

        public string BuildIsyeriIdListCsv(int firmaId, List<FirmaIsyeriYetkiDTO> yetkiler, bool isAdmin, IReadOnlyList<int> firmaIsyeriIds)
        {
            var allIds = firmaIsyeriIds?.ToList() ?? new List<int>();
            if (!allIds.Contains(0))
                allIds.Add(0);

            if (isAdmin || yetkiler == null || yetkiler.Count == 0)
                return allIds.Count > 0 ? string.Join(",", allIds) : null;

            var firmaYetkileri = yetkiler.Where(y => y.FirmaId == firmaId).ToList();
            if (firmaYetkileri.Count == 0)
                return null;

            if (firmaYetkileri.Any(y => !y.IsyeriId.HasValue))
                return allIds.Count > 0 ? string.Join(",", allIds) : null;

            var yetkiliIds = firmaYetkileri
                .Where(y => y.IsyeriId.HasValue)
                .Select(y => y.IsyeriId!.Value)
                .Distinct()
                .ToList();

            if (!yetkiliIds.Contains(0))
                yetkiliIds.Add(0);

            return yetkiliIds.Count > 0 ? string.Join(",", yetkiliIds) : null;
        }
    }
}
