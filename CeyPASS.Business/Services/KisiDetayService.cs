using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Services
{
    /// <summary>Personel detay sorgusu.</summary>
    public sealed class KisiDetayService:IKisiDetayService
    {
        private readonly IKisiRepository _repo;

        public KisiDetayService(IKisiRepository repo)
        {
                _repo= repo;
        }
        /// <inheritdoc />
        public KisiDetayDTO GetDetay(int kisiId)
        {
            return _repo.GetById(kisiId);
        }
    }
}
