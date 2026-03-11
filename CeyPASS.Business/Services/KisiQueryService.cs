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

        public List<KisiListItem> GetAktifKisilerByFirma(int firmId, string? search = null, bool? puantajYapilirMi = true, int? isyeriId = null)
        {
            return _repo.GetAktifByFirma(firmId, search, puantajYapilirMi, isyeriId);
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
