using System;

namespace CeyPASS.Entities.Concrete
{
    public class KisiDetay
    {
        public string PersonelId { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string KartNo { get; set; }
        public string TcKimlikNo { get; set; }
        public int? PozisyonId { get; set; }
        public int? DepartmanId { get; set; }
        public int FirmaId { get; set; }
        public int? IsyeriId { get; set; }
        public int? BolumId { get; set; }
        public DateTime? DogumTarihi { get; set; }
        public DateTime? IseGirisTarihi { get; set; }
        public DateTime? IstenCikisTarihi { get; set; }
        public int? CalismaStatusuId { get; set; }
        public string CalismaStatusuText { get; set; }
        public string CalismaSekliCsv { get; set; }
        public string CepTel { get; set; }
        public string Email { get; set; }
        public byte[] Fotograf { get; set; }
        public bool FirmaPersoneli { get; set; }
        public bool PuantajYapilabilir { get; set; }
        public bool YemekHakkiVar { get; set; }
        public int? GunlukYemekAdedi { get; set; }
        public string TaseronKartNo { get; set; }          
    }
}
