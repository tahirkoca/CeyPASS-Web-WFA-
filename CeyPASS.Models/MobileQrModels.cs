namespace CeyPASS.Models
{
    /// <summary>Mobil QR geçiş isteği; konum cihaz toleransı ile doğrulanır.</summary>
    public class QrIstekModel
    {
        public int CihazId { get; set; }
        public double? Enlem { get; set; }
        public double? Boylam { get; set; }
        /// <summary>Sahte konum (mock) kullanıldıysa true; güvenlik kontrollerinde dikkate alınır.</summary>
        public bool IsMocked { get; set; }
    }
}
