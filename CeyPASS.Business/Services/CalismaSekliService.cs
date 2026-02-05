using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class CalismaSekliService : ICalismaSekliService
    {
        private readonly ICalismaSekliRepository _repo;

        public CalismaSekliService(ICalismaSekliRepository repo)
        {
            _repo = repo;
        }
        public List<CalismaSekli> GetAll(int firmaId, bool includeGlobal = true) => _repo.GetAll(firmaId, includeGlobal);
        public List<CalismaSekli> GetAllForAdmin() => _repo.GetAllForAdmin();
        public int Add(CalismaSekli x) => _repo.Insert(x);
        public bool Update(CalismaSekli x) => _repo.Update(x);
        public bool Delete(int id, int firmaId) => _repo.Delete(id, firmaId);
    }
}
