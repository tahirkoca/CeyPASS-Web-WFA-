namespace CeyPASS.Entities.Concrete
{
    /// <summary>Token ile yeni şifre belirleme adımı sonucu.</summary>
    public class SifreSifirlamaTamamlayici
    {
        public bool Basarili { get; set; }
        public string HataMesaji { get; set; }
    }
}
