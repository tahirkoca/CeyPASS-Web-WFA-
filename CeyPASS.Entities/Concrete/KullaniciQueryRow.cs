namespace CeyPASS.Entities.Concrete
{
    public class KullaniciQueryRow
    {
        public string KullaniciId { get; set; }
        public string KullaniciAdi { get; set; }
        public string Sifre { get; set; }
        public int? RolId { get; set; }
        public int? PersonelId { get; set; }
        public string RolTanimi { get; set; }
        public string AdSoyad { get; set; }
        public int? FirmaId { get; set; }
        public string FirmaAdi { get; set; }
        public string Email { get; set; }
    }
}
