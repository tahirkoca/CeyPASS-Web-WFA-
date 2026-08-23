namespace CeyPASS.Entities.Concrete
{
    public class KisiKayitValidasyonDTO
    {
        public string PersonelId { get; set; }
        public bool FirmaPersoneli { get; set; }
        public bool PuantajYapilir { get; set; }
        public bool YemekHakkiVar { get; set; }
        public int YemekAdedi { get; set; }
        public string FirmaDisiKartNo { get; set; }
        public string TcKimlikNo { get; set; }
        public string KartNo { get; set; }
        public bool TaseronCalisanMi { get; set; }
        public bool ZiyaretciMi { get; set; }
        public bool AracKartiMi { get; set; }
    }
}
