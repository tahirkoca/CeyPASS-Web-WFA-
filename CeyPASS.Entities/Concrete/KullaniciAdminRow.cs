namespace CeyPASS.Entities.Concrete
{
    /// <summary>Kullanıcı yönetim grid satırı.</summary>
    public class KullaniciAdminRow
    {
        public int KullaniciId { get; set; }
        public string KullaniciAdi { get; set; } = "";
        public int? RolId { get; set; }
        public string? RolTanimi { get; set; }
        public int? PersonelId { get; set; }
    }
}

