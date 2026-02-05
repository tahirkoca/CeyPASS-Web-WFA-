using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Services
{
    public class CanliIzlemeService:ICanliIzlemeService
    {
        private readonly IFirmaRepository _firmaRepo;
        private readonly ICanliIzlemeRepository _canliRepo;

        public CanliIzlemeService(IFirmaRepository firmaRepo,ICanliIzlemeRepository canliIzlemeRepo)
        {
            _firmaRepo = firmaRepo;
            _canliRepo= canliIzlemeRepo;
        }
        public DataTable GetFirmalar() => _firmaRepo.GetFirmalar();
        public AuthUserDTO Login(int firmaId, string user, string pass) =>_canliRepo.Validate(firmaId, user, pass);
        public List<LastPassDTO> GetLastPasses(int firmaId, int take)
        {
            return _canliRepo.GetLastPasses(firmaId, take);
        }
        public List<LastPassDTO> GetLastPassesYemekhane(int firmaId, int take)
        {
            return _canliRepo.GetLastPassesYemekhane(firmaId, take);
        }
    }
}
