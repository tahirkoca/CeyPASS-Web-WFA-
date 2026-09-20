using CeyPASS.Entities.Concrete;
using System.Collections.Generic;
using System.Data;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Tanımlı rapor çalıştırma ve parametre sorguları.</summary>
    public interface IRaporRepository
    {
        /// <summary>Raporlari Getir işlemini gerçekleştirir.</summary>
        List<RaporTanimi> RaporlariGetir();
        /// <summary>Procedure Parameter Names sorgularını getirir.</summary>
        IReadOnlyList<string> GetProcedureParameterNames(string procedureAdi);
        /// <summary>Raporu Calistir işlemini gerçekleştirir.</summary>
        DataTable RaporuCalistir(string procedureAdi, Dictionary<string, object> parametreler);
    }
}
