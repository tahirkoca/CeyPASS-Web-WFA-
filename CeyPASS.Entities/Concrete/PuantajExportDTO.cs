namespace CeyPASS.Entities.Concrete
{
    public class PuantajExportDTO
    {
        public string SicilNo { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string Firma { get; set; }
        public string Bolum { get; set; }
        public string Isyeri { get; set; }
        public decimal NormalSaat { get; set; }
        public int HaftaTatiliGun { get; set; }
        public int ResmiTatilGun { get; set; }
        public int UcretliIzinGun { get; set; }
        public decimal UcretsizIzinSaat { get; set; }
        public int RaporGun { get; set; }
        public decimal FazlaMesaiSaat { get; set; }
        public decimal FazlaCalismaSaat { get; set; }
        public string FazlaMesai125Saat { get; set; }
        public decimal ResmiTatilSaat { get; set; }
        public string SskEksikNedeni { get; set; }
        public string IskurEksikNedeni { get; set; }
    }
}
