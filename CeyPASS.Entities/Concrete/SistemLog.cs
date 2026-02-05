namespace CeyPASS.Entities.Concrete
{
    public class SistemLog
    {
        public int? KullaniciId { get; set; }
        public IslemTuru IslemTuru { get; set; }
        public string Kaynak { get; set; }   
        public string Islem { get; set; }   
        public string Mesaj { get; set; }
        public string IpAdres { get; set; }
        public string BilgisayarAdi { get; set; }
        public string KorelasyonId { get; set; }
        public string DetayJson { get; set; } 
        public string HataMesaji { get; set; }
    }
}
