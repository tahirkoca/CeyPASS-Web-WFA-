using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.DataAccess.Abstractions;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    /// <summary>Pozisyon CRUD.</summary>
    public class PozisyonService:IPozisyonService
    {
        private readonly IPozisyonRepository _repo;

        public PozisyonService(IPozisyonRepository repo)
        {
            _repo=repo;
        }
        /// <inheritdoc />
        public List<LookupItem> GetAll() => _repo.GetAll();
        /// <inheritdoc />
        public List<PozisyonListDTO> GetListForAdmin() => _repo.GetListForAdmin();
        public (int id, string ad, string ack)? GetForEdit(int id)
        {
            var row = _repo.GetById(id);
            if (row == null) return null;
            return ((int)row["PozisyonId"], row["PozisyonAdi"] + "", row["Aciklama"] + "");
        }
        /// <inheritdoc />
        public bool Add(string ad, string aciklama) => _repo.Insert(ad, aciklama);
        /// <inheritdoc />
        public bool Update(int id, string ad, string aciklama) => _repo.Update(id, ad, aciklama);
        /// <inheritdoc />
        public bool Delete(int id) => _repo.Delete(id);
    }
}
