using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.DataAccess.Abstractions;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    /// <summary>Çalışma statüsü lookup CRUD.</summary>
    public class CalismaStatuService:ICalismaStatuService
    {
        private readonly ICalismaStatuRepository _repo;

        public CalismaStatuService(ICalismaStatuRepository repo)
        {
            _repo = repo;
        }
        /// <inheritdoc />
        public List<LookupItem> GetAll() => _repo.GetByFirma();
        /// <inheritdoc />
        public int GetNextId() => _repo.GetNextId();
        /// <inheritdoc />
        public bool Add(int id, string ad) => _repo.Insert(id, ad);
        /// <inheritdoc />
        public bool AddAuto(string ad) => _repo.Insert(_repo.GetNextId(), ad);
        /// <inheritdoc />
        public bool Update(int id, string ad) => _repo.Update(id, ad);
        /// <inheritdoc />
        public bool Delete(int id) => _repo.Delete(id);
    }
}
