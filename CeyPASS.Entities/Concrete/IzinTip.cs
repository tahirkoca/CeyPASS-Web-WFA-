namespace CeyPASS.Entities.Concrete
{
    /// <summary>İzin türü; Logo/puantaj kodları ve ücretli/saatlik bayrakları.</summary>
    public class IzinTip
    {
        public int IzinTipId { get; set; }
        /// <summary>Entegrasyon ve raporlama kodu.</summary>
        public string Kod { get; set; }
        public string Ad { get; set; }
        public bool UcretliMi { get; set; }
        public bool AktifMi { get; set; }
        public bool SaatlikKullanilabilirMi { get; set; }
    }
}
