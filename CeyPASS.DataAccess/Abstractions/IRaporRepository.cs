using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IRaporRepository
    {
        List<RaporTanimi> RaporlariGetir();
        IReadOnlyList<string> GetProcedureParameterNames(string procedureAdi);
        DataTable RaporuCalistir(string procedureAdi, Dictionary<string, object> parametreler);
    }
}
