using System;

namespace CeyPASS.Entities.Concrete
{
    /// <summary>Puantaj gün düzenleme/kaydetme DTO; FM ve tolerans dakikaları.</summary>
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
        /// <summary>Hareketlerden hesaplanan fazla mesai (dakika).</summary>
        public int SistemFMDakika { get; set; }              
        public OnayDurumu OnayDurumu { get; set; } = OnayDurumu.Bekliyor;
        /// <summary>İK/manuel override fazla mesai (dakika).</summary>
        public int DuzenlenenFMDakika { get; set; } = 0;     
        public string Aciklama { get; set; }
        public string CalismaTipi { get; set; }
        public decimal Saat { get; set; }
    }

}
