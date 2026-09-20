using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    /// <summary>Çoklu sicil bağlantı işlemleri.</summary>
    public class CokluSicilService : ICokluSicilService
    {
        private readonly ICokluSicilRepository _repo;

        public CokluSicilService(ICokluSicilRepository repo)
        {
            _repo = repo;
        }

        /// <inheritdoc />
        public List<CokluSicilBaglantiDTO> GetByAnaPersonelId(int anaPersonelId, bool yalnizcaAktif = false)
            => _repo.GetByAnaPersonelId(anaPersonelId, yalnizcaAktif);

        /// <inheritdoc />
        public List<CokluSicilHedefAdayDTO> GetHedefAdaylari(int anaPersonelId, string tcKimlikNo)
            => _repo.GetHedefAdaylari(tcKimlikNo, anaPersonelId);

        /// <inheritdoc />
        public CokluSicilOzetDTO GetOzet(int personelId)
            => _repo.GetOzet(personelId);

        /// <inheritdoc />
        public void Upsert(int anaPersonelId, string tcKimlikNo, CokluSicilUpsertRequest request, int? kullaniciId)
            => _repo.Upsert(request, anaPersonelId, tcKimlikNo, kullaniciId);

        /// <inheritdoc />
        public void SetAktif(int anaPersonelId, int hedefPersonelId, bool aktif, int? kullaniciId)
            => _repo.SetAktif(anaPersonelId, hedefPersonelId, aktif, kullaniciId);

        /// <inheritdoc />
        public void PasifleştirTümünü(int anaPersonelId, int? kullaniciId)
            => _repo.PasifleştirTümünü(anaPersonelId, kullaniciId);

        /// <inheritdoc />
        public bool IsAnaSicil(int personelId) => _repo.IsAnaSicil(personelId);

        /// <inheritdoc />
        public bool IsHedefSicil(int personelId) => _repo.IsHedefSicil(personelId);

        /// <inheritdoc />
        public int GetAktifHedefSayisi(int personelId) => _repo.GetAktifHedefSayisi(personelId);
    }
}
