using System;
using System.Collections.Generic;

namespace CeyPASS.Entities.Concrete
{
    public class GuncellemeNotifikasyonDTO
    {
        public GuncellemeNotifikasyonDTO()
        {
            YeniOzellikler = new List<string>();
            HataDuzeltmeleri = new List<string>();
            Iyilestirmeler = new List<string>();
            KritikDegisiklikler = new List<string>();
        }
        public string VersiyonNumarasi { get; set; }
        public DateTime YayinTarihi { get; set; }
        public string GuncellemeTipi { get; set; }
        public List<string> YeniOzellikler { get; set; }
        public List<string> HataDuzeltmeleri { get; set; }
        public List<string> Iyilestirmeler { get; set; }
        public List<string> KritikDegisiklikler { get; set; }
        public string EkNotlar { get; set; }
    }
}
