namespace CeyPASS.Entities.Concrete
{
    public class IzinTip
    {
        public int IzinTipId { get; set; }
        public string Kod { get; set; }
        public string Ad { get; set; }
        public bool UcretliMi { get; set; }
        public bool AktifMi { get; set; }
        public bool SaatlikKullanilabilirMi { get; set; }
    }
}
