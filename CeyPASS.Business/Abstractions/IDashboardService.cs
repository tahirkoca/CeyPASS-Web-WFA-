using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Abstractions
{
    /// <summary>Anlık geçiş ve aylık özet dashboard verisi.</summary>
    public interface IDashboardService
    {
        /// <summary>Çok firmalı dashboard; tolerans ve anlık limit parametreleri ile.</summary>
        DashboardResult GetDashboard(IEnumerable<int> firmaIdList, DateTime gun, DateTime ayBas, DateTime aySon, double tolBasSaat, double tolBitSaat, int anlikLimit);

        /// <summary>Bugün için tek firma dashboard özeti.</summary>
        DashboardResult GetDashboardForToday(int firmaId);
    }
}
