using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Services
{
    public sealed class KisiDetayService:IKisiDetayService
    {
        private readonly IKisiRepository _repo;

        public KisiDetayService(IKisiRepository repo)
        {
                _repo= repo;
        }
        public KisiDetayDTO GetDetay(int kisiId)
        {
            return _repo.GetById(kisiId);
        }
    }
}
