using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    public interface IDashboardService
    {
        DashboardResult GetDashboard(IEnumerable<int> firmaIdList, DateTime gun, DateTime ayBas, DateTime aySon, double tolBasSaat, double tolBitSaat, int anlikLimit);
        DashboardResult GetDashboardForToday(int firmaId);
    }
}
