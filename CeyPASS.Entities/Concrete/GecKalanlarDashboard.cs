namespace CeyPASS.Entities.Concrete
{
    /// <summary>Dashboard geç kalanlar listesi satırı.</summary>
    public class GecKalanlarDashboard
    {
        public int PersonelId { get; set; }
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public int FirmaId { get; set; }
        public int IsyeriId { get; set; }
        public string FirmaAdi { get; set; } = "";
        public string IsyeriAdi { get; set; } = "";
        /// <summary>Vardiya başlangıcına göre gecikme (dakika).</summary>
        public int FazlaDakika { get; set; }
    }
}
