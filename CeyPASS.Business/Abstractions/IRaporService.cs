using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Tanımlı raporlar ve stored procedure çalıştırma.</summary>
    public interface IRaporService
    {
        /// <summary>Kullanılabilir rapor tanımları.</summary>
        List<RaporTanimi> GetirRaporlar();

        /// <summary>SP parametre adları (form oluşturma).</summary>
        IReadOnlyList<string> GetProcedureParameterNames(string procedureAdi);

        /// <summary>Parametre sözlüğü ile rapor SP çalıştırır.</summary>
        DataTable CalistirRapor(string procedureAdi, Dictionary<string, object> parametreler);
    }
}
