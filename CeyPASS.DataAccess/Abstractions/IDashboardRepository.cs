using CeyPASS.Entities.Concrete;
using System;

namespace CeyPASS.DataAccess.Abstractions
{
    /// <summary>Yönetim paneli özet istatistik sorguları.</summary>
    public interface IDashboardRepository
    {
        /// <summary>Execute Dashboard işlemini gerçekleştirir.</summary>
        DashboardResult ExecuteDashboard(string firmaIdCsv, DateTime gun, DateTime ayBas, DateTime aySon, double tolBasSaat, double tolBitSaat, int anlikLimit);
    }
}
