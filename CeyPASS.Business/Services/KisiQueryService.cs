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

        public List<KisiListItem> GetAktifKisilerByFirma(int firmId, string? search = null, bool? puantajYapilirMi = true, int? isyeriId = null, IReadOnlyList<int>? isyeriIdIn = null, bool sadeceIstenCikanlar = false)
        {
            // aracKartiMi named: positional sadeceIstenCikanlar would otherwise bind to aracKartiMi
            return _repo.GetAktifByFirma(
                firmId,
                search,
                puantajYapilirMi,
                isyeriId,
                isyeriIdIn,
                ziyaretciMi: null,
                aracKartiMi: null,
                sadeceIstenCikanlar: sadeceIstenCikanlar);
        }

        public List<KisiListItem> GetAktifKisilerByFirmaPaged(int firmId, string? search, bool? puantajYapilirMi, int? isyeriId, IReadOnlyList<int>? isyeriIdIn, bool sadeceIstenCikanlar, int page, int pageSize, out int totalCount)
        {
            return _repo.GetAktifByFirmaPaged(firmId, search, puantajYapilirMi, isyeriId, isyeriIdIn, sadeceIstenCikanlar, page, pageSize, out totalCount);
        }

        public List<KisiSearchResultItem> SearchKisilerPaged(KisiSearchFilter filter, int page, int pageSize, out int totalCount)
        {
            return _repo.SearchByFirmaPaged(filter, page, pageSize, out totalCount);
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
