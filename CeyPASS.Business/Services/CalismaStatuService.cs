using CeyPASS.Business.Abstractions;
using CeyPASS.Entities.Concrete;
using CeyPASS.DataAccess.Abstractions;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class CalismaStatuService:ICalismaStatuService
    {
        private readonly ICalismaStatuRepository _repo;

        public CalismaStatuService(ICalismaStatuRepository repo)
        {
            _repo = repo;
        }
        public List<LookupItem> GetAll() => _repo.GetByFirma();
        public int GetNextId() => _repo.GetNextId();
        public bool Add(int id, string ad) => _repo.Insert(id, ad);
        public bool AddAuto(string ad) => _repo.Insert(_repo.GetNextId(), ad);
        public bool Update(int id, string ad) => _repo.Update(id, ad);
        public bool Delete(int id) => _repo.Delete(id);
    }
}
