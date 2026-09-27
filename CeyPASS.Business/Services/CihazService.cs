using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    /// <summary>Geçiş cihazı yönetimi.</summary>
    public class CihazService:ICihazService
    {
        private readonly ICihazRepository _repo;

        public CihazService(ICihazRepository repo)
        {
            _repo = repo;
        }
        /// <inheritdoc />
        public List<CihazListDTO> GetListe(bool sadeceAktif, int? firmaId = null, bool sadeceYemekhane = false)
            => _repo.GetList(sadeceAktif, firmaId, sadeceYemekhane);
        /// <inheritdoc />
        public Cihaz Get(int id) => _repo.GetById(id);
        /// <inheritdoc />
        public int Ekle(Cihaz c) => _repo.Insert(c);
        /// <inheritdoc />
        public void Guncelle(Cihaz c) => _repo.Update(c);
        /// <inheritdoc />
        public void PasifYap(int id) => _repo.SetAktif(id, false);
        /// <inheritdoc />
        public void AktifYap(int id) => _repo.SetAktif(id, true);
        /// <inheritdoc />
        public List<CihazTip> GetCihazTipleri() => _repo.GetTips();
    }
}
