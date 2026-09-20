using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Services
{
    /// <summary>Canlı izleme oturum ve son geçişler.</summary>
    public class CanliIzlemeService:ICanliIzlemeService
    {
        private readonly IFirmaRepository _firmaRepo;
        private readonly ICanliIzlemeRepository _canliRepo;

        public CanliIzlemeService(IFirmaRepository firmaRepo,ICanliIzlemeRepository canliIzlemeRepo)
        {
            _firmaRepo = firmaRepo;
            _canliRepo= canliIzlemeRepo;
        }
        /// <inheritdoc />
        public DataTable GetFirmalar() => _firmaRepo.GetFirmalar();
        /// <inheritdoc />
        public AuthUserDTO Login(int firmaId, string user, string pass) =>_canliRepo.Validate(firmaId, user, pass);
        /// <inheritdoc />
        public List<LastPassDTO> GetLastPasses(int firmaId, int take)
        {
            return _canliRepo.GetLastPasses(firmaId, take);
        }
        /// <inheritdoc />
        public List<LastPassDTO> GetLastPassesYemekhane(int firmaId, int take)
        {
            return _canliRepo.GetLastPassesYemekhane(firmaId, take);
        }
        /// <inheritdoc />
        public List<LastPassDTO> GetLastPassesArac(int firmaId, int take)
        {
            return _canliRepo.GetLastPassesArac(firmaId, take);
        }
        /// <inheritdoc />
        public List<string> GetKullaniciAdlariByFirma(int firmaId)
        {
            return _canliRepo.GetKullaniciAdlariByFirma(firmaId);
        }
    }
}
