namespace CeyPASS.Entities.Concrete
{
    /// <summary>Kişi Ara modal sonuç satırı.</summary>
    public class KisiSearchResultItem
    {
        public string PersonelId { get; set; } = "";
        public string AdSoyad { get; set; } = "";
        public string KartNo { get; set; } = "";
        public string TcKimlikNo { get; set; } = "";
        public string IsyeriAdi { get; set; } = "";
        public string PozisyonAdi { get; set; } = "";
    }
}
