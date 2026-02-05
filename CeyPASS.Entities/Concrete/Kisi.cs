using System;

namespace CeyPASS.Entities.Concrete
{
    public class Kisi
    {
        public string PersonelId { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string KartNo { get; set; }
        public string TcKimlikNo { get; set; }
        public int? PozisyonId { get; set; }
        public DateTime? DogumTarihi { get; set; }
        public int? DepartmanId { get; set; }
        public DateTime IseGirisTarihi { get; set; }
        public DateTime? IstenCikisTarihi { get; set; }
        public string CalismaStatusu { get; set; }
        public int FirmaId { get; set; }             
        public int? IsyeriId { get; set; }
        public int? BolumId { get; set; }
        public string CalismaSekli { get; set; }
        public string CepTel { get; set; }
        public byte[] Fotograf { get; set; }
        public DateTime? KayitTarihi { get; set; }
        public string Email { get; set; }
        public bool PuantajYapilirMi { get; set; }
        public string AdSoyad { get { return ((Ad ?? "").Trim() + " " + (Soyad ?? "").Trim()).Trim(); } }
    }

}
