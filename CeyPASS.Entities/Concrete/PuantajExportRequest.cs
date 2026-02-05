using System.Collections.Generic;

namespace CeyPASS.Entities.Concrete
{
    public class PuantajExportRequest
    {
        public int Yil { get; set; }
        public int Ay { get; set; }
        public List<FirmaIsyeriYetkiDTO> Yetkiler { get; set; }
    }
}
