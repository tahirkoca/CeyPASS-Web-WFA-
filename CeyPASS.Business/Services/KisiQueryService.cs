using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class KisiQueryService : IKisiQueryService
    {
        private readonly IKisiRepository _repo;

        public KisiQueryService(IKisiRepository repo)
        {
            _repo = repo;
        }

        public List<KisiListItem> GetAktifKisilerByFirma(int firmId, string? search = null, bool? puantajYapilirMi = true, int? isyeriId = null, IReadOnlyList<int>? isyeriIdIn = null)
        {
            return _repo.GetAktifByFirma(firmId, search, puantajYapilirMi, isyeriId, isyeriIdIn);
        }

        public List<KisiListItem> GetAktifKisilerByFirmaPaged(int firmId, string? search, bool? puantajYapilirMi, int? isyeriId, IReadOnlyList<int>? isyeriIdIn, int page, int pageSize, out int totalCount)
        {
            return _repo.GetAktifByFirmaPaged(firmId, search, puantajYapilirMi, isyeriId, isyeriIdIn, page, pageSize, out totalCount);
        }

        public KisiDetay GetKisiDetay(string personelId)
        {
            return _repo.GetDetay(personelId);
        }

        public (KisiDetay? detay, bool isPuantajsizKart) GetDetayOrPuantajsizKart(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return (null, false);

            var kisi = _repo.GetDetay(id);
            return (kisi, false);
        }
    }
}
