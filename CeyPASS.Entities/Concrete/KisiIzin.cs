using System;

namespace CeyPASS.Entities.Concrete
{
    public class KisiIzin
    {
        public int? KisiIzinId { get; set; }          
        public int FirmaId { get; set; }
        public string PersonelId { get; set; }        
        public int IzinId { get; set; }               
        public DateTime Baslangic { get; set; }
        public DateTime Bitis { get; set; }
        public int SureDakika { get; set; }           
        public string Aciklama { get; set; }
        public bool SaatlikIzinMi { get; set; }
        public int OlusturanKullaniciId { get; set; }
    }
}
