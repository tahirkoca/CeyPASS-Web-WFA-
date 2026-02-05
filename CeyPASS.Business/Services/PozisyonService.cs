using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.DataAccess.Abstractions;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class PozisyonService:IPozisyonService
    {
        private readonly IPozisyonRepository _repo;

        public PozisyonService(IPozisyonRepository repo)
        {
            _repo=repo;
        }
        public List<LookupItem> GetAll() => _repo.GetAll();
        public List<PozisyonListDTO> GetListForAdmin() => _repo.GetListForAdmin();
        public (int id, string ad, string ack)? GetForEdit(int id)
        {
            var row = _repo.GetById(id);
            if (row == null) return null;
            return ((int)row["PozisyonId"], row["PozisyonAdi"] + "", row["Aciklama"] + "");
        }
        public bool Add(string ad, string aciklama) => _repo.Insert(ad, aciklama);
        public bool Update(int id, string ad, string aciklama) => _repo.Update(id, ad, aciklama);
        public bool Delete(int id) => _repo.Delete(id);
    }
}
