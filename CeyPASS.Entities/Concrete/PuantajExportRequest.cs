using System.Collections.Generic;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Puantaj dışa aktarımı için dönem ve firma/işyeri yetki filtresi.</summary>
    public class PuantajExportRequest
    {
        public int Yil { get; set; }
        public int Ay { get; set; }
        public List<FirmaIsyeriYetkiDTO> Yetkiler { get; set; }
    }
}
