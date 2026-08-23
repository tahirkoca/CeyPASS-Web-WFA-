using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    public interface IRaporService
    {
        List<RaporTanimi> GetirRaporlar();
        IReadOnlyList<string> GetProcedureParameterNames(string procedureAdi);
        DataTable CalistirRapor(string procedureAdi, Dictionary<string, object> parametreler);
    }
}
