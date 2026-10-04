namespace CeyPASS.Entities.Concrete
{
    /// <summary>Cihaz seçim listesi; bağlantı ve görünen metin alanları.</summary>
    public class CihazListDTO
    {
        public int CihazId { get; set; }
        public int FirmaId { get; set; }
        public string CihazAdi { get; set; }
        public string IPAdres { get; set; }
        public int Port { get; set; }
        public string FirmaAdi { get; set; }
        public bool AktifMi { get; set; }
        public bool AnaGirisCikisMi { get; set; }
        /// <summary>0 = Çıkış, 1 = Giriş (AnaGirisCikisMi cihazlarda).</summary>
        public int CihazTipi { get; set; }
        public string Text { get; set; }
        public override string ToString() => $"{IPAdres ?? ""} {CihazAdi ?? ""}" + (string.IsNullOrWhiteSpace(FirmaAdi) ? "" : $" [{FirmaAdi}]");
    }
}
