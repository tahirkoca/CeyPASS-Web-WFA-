using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class CihazService:ICihazService
    {
        private readonly ICihazRepository _repo;

        public CihazService(ICihazRepository repo)
        {
            _repo = repo;
        }
        public List<CihazListDTO> GetListe(bool sadeceAktif, int? firmaId = null) => _repo.GetList(sadeceAktif, firmaId);
        public Cihaz Get(int id) => _repo.GetById(id);
        public int Ekle(Cihaz c) => _repo.Insert(c);
        public void Guncelle(Cihaz c) => _repo.Update(c);
        public void PasifYap(int id) => _repo.SetAktif(id, false);
        public void AktifYap(int id) => _repo.SetAktif(id, true);
        public List<CihazTip> GetCihazTipleri() => _repo.GetTips();
    }
}
