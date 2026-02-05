using System.Collections.Generic;
using System.Data;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Services
{
    public class RaporService:IRaporService
    {
        private readonly IRaporRepository _repo;

        public RaporService(IRaporRepository repo)
        {
            _repo = repo;
        }
        public List<RaporTanimi> GetirRaporlar()
        {
            return _repo.RaporlariGetir();
        }
        public DataTable CalistirRapor(string procedureAdi, Dictionary<string, object> parametreler)
        {
            return _repo.RaporuCalistir(procedureAdi, parametreler);
        }
    }

}
