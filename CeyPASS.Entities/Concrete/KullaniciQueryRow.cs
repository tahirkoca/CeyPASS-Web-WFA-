namespace CeyPASS.Entities.Concrete
{
    /// <summary>SqlQueryRaw sonucu — LEFT JOIN kolonları NULL olabilir.</summary>
    public class KullaniciQueryRow
    {
        public string? KullaniciId { get; set; }
        public string? KullaniciAdi { get; set; }
        public string? Sifre { get; set; }
        public int? RolId { get; set; }
        public int? PersonelId { get; set; }
        public string? RolTanimi { get; set; }
        public string? AdSoyad { get; set; }
        public int? FirmaId { get; set; }
        public string? FirmaAdi { get; set; }
        public string? Email { get; set; }
    }
}
