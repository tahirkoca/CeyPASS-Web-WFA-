namespace CeyPASS.Entities.Concrete
{
    /// <summary>Çok kiracılı yapıda üst firma tanımı.</summary>
    public class Firma
    {
        public int FirmaId { get; set; }
        public string FirmaAdi { get; set; }
        /// <summary>Şifre sıfırlama ve IT bildirimleri için.</summary>
        public string ITBirimMail { get; set; }
    }
}
