using System.Collections.Generic;
using System.Data;
using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;

namespace CeyPASS.Business.Services
{
    /// <summary>Rapor SP çalıştırma.</summary>
    public class RaporService : IRaporService
    {
        private readonly IRaporRepository _repo;

        public RaporService(IRaporRepository repo)
        {
            _repo = repo;
        }

        /// <inheritdoc />
        public List<RaporTanimi> GetirRaporlar()
        {
            return _repo.RaporlariGetir();
        }

        /// <inheritdoc />
        public IReadOnlyList<string> GetProcedureParameterNames(string procedureAdi)
        {
            return _repo.GetProcedureParameterNames(procedureAdi);
        }

        /// <inheritdoc />
        public DataTable CalistirRapor(string procedureAdi, Dictionary<string, object> parametreler)
        {
            return _repo.RaporuCalistir(procedureAdi, parametreler);
        }
    }
}
