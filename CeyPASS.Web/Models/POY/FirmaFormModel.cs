namespace CeyPASS.Web.Models.POY
{
    /// <summary>Firma POY form modeli.</summary>
    public class FirmaFormModel
    {
        public int FirmaId { get; set; }
        public string FirmaAdi { get; set; }
        public string ITBirimMail { get; set; }
        /// <summary>İşlem sonrası yönlendirilecek URL (örn. Admin panel).</summary>
        public string ReturnUrl { get; set; }
    }
}

