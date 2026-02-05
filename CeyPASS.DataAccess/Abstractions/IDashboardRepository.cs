using CeyPASS.Entities.Concrete;
using System;

namespace CeyPASS.DataAccess.Abstractions
{
    public interface IDashboardRepository
    {
        DashboardResult ExecuteDashboard(string firmaIdCsv, DateTime gun, DateTime ayBas, DateTime aySon, double tolBasSaat, double tolBitSaat, int anlikLimit);
    }
}
