using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;

namespace CeyPASS.Business.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repo;

        public DashboardService(IDashboardRepository repo)
        {
            _repo = repo;
        }
        public DashboardResult GetDashboard(IEnumerable<int> firmaIdList, DateTime gun, DateTime ayBas, DateTime aySon, double tolBasSaat, double tolBitSaat, int anlikLimit)
        {
            string firmaCsv = string.Join(",", firmaIdList ?? Array.Empty<int>());
            return _repo.ExecuteDashboard(firmaCsv, gun, ayBas, aySon, tolBasSaat, tolBitSaat, anlikLimit);
        }
        public DashboardResult GetDashboardForToday(int firmaId)
        {
            var today = DateTime.Today;
            var ayBas = new DateTime(today.Year, today.Month, 1);
            var aySon = new DateTime(today.Year, today.Month,DateTime.DaysInMonth(today.Year, today.Month));

            const double tolBas = 0.25;
            const double tolBit = 0.25;
            const int anlikLimit = 30;

            return GetDashboard(new List<int> { firmaId }, today, ayBas, aySon, tolBas, tolBit, anlikLimit);
        }
    }
}
