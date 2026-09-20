namespace CeyPASS.Entities.Concrete
{
    /// <summary>Cihaz yönetim grid satırı.</summary>
    public class CihazListRow
    {
        public int CihazId { get; set; }
        public int FirmaId { get; set; }
        public string CihazAdi { get; set; }
        public string IPAdres { get; set; }
        public string FirmaAdi { get; set; }
    }
}
