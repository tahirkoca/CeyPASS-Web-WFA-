using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    /// <summary>Kullanıcı ilişkili sorgular.</summary>
    public class KullaniciQueryService:IKullaniciQueryService
    {
        private readonly IKullaniciRepository _repo;

        public KullaniciQueryService(IKullaniciRepository repo)
        {
            _repo = repo;
        }
        /// <inheritdoc />
        public List<int> GetFirmayaAitIsyeriIdleri(int firmaId)=> _repo.GetIsyeriIdListByFirma(firmaId);
    }
}
