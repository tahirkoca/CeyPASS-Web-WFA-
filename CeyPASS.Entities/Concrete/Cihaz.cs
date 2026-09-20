namespace CeyPASS.Entities.Concrete
{
    /// <summary>Turnike/terminal cihazı; QR ve canlı izleme kaynağı ayarları.</summary>
    public class Cihaz
    {
        public int CihazId { get; set; }          
        public int FirmaId { get; set; }
        public string CihazAdi { get; set; }
        public string IPAdres { get; set; }
        /// <summary>ZK varsayılan portu.</summary>
        public int Port { get; set; } = 4370;
        public string Notlar { get; set; }       
        public int CihazTipi { get; set; }        
        public bool AktifMi { get; set; } = true;
        public bool BaglandiMi { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        /// <summary>Mobil QR geçişinde izin verilen mesafe (metre).</summary>
        public int? MesafeToleransMetre { get; set; }
        public bool SaatPenceresiAktifMi { get; set; }
        /// <summary>Danışma / normal Canlı İzleme kaynağı.</summary>
        public bool AnaGirisCikisMi { get; set; }
        /// <summary>ARAÇ rolü Canlı İzleme kaynağı.</summary>
        public bool AracGirisCikisMi { get; set; }
    }
}
