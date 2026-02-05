using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    public interface IRaporService
    {
        List<RaporTanimi> GetirRaporlar();
        DataTable CalistirRapor(string procedureAdi, Dictionary<string, object> parametreler);
    }
}
