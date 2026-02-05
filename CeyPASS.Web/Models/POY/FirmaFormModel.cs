namespace CeyPASS.Web.Models.POY
{
    public class FirmaFormModel
    {
        public int FirmaId { get; set; }
        public string FirmaAdi { get; set; }
        public string ITBirimMail { get; set; }
        /// <summary>İşlem sonrası yönlendirilecek URL (örn. Admin panel).</summary>
        public string ReturnUrl { get; set; }
    }
}

