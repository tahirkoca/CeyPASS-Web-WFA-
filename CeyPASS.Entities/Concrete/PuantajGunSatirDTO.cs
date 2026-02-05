using System;

namespace CeyPASS.Entities.Concrete
{
    public class PuantajGunSatirDTO
    {
        public DateTime Tarih { get; set; }
        public string VardiyaTuru { get; set; }           
        public TimeSpan? IlkGiris { get; set; }
        public TimeSpan? SonCikis { get; set; }
        public TimeSpan? VardiyaBaslangic { get; set; }
        public TimeSpan? VardiyaBitis { get; set; }
        public int SaatlikIzinDakika { get; set; }
        public int ErkenGirisDakika { get; set; }            
        public int GecCikisDakika { get; set; }               
        public int SistemFMDakika { get; set; }              
        public OnayDurumu OnayDurumu { get; set; } = OnayDurumu.Bekliyor;
        public int DuzenlenenFMDakika { get; set; } = 0;     
        public string Aciklama { get; set; }
        public string CalismaTipi { get; set; }
        public decimal Saat { get; set; }
    }

}
