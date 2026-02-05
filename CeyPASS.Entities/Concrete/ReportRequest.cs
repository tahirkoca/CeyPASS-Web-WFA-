using System;

namespace CeyPASS.Entities.Concrete
{
    public class ReportRequest
    {
        public DashboardReportTypeHelper Type { get; set; }
        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
        public int? FirmaId { get; set; }
    }
}
