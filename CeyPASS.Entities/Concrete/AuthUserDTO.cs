namespace CeyPASS.Entities.Concrete
{
    public class AuthUserDTO
    {
        public int KullaniciId { get; set; }
        public int FirmaId { get; set; }
        public string? FirmaAdi { get; set; }
        public string? KullaniciAdi { get; set; }
        public string? AdSoyad { get; set; }
        public string? Rol { get; set; }
        public int? RolId { get; set; }
    }
}
