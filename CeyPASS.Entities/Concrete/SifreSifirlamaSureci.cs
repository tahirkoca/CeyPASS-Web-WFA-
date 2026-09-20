namespace CeyPASS.Entities.Concrete
{
    /// <summary>Şifre sıfırlama e-posta gönderim adımı sonucu.</summary>
    public class SifreSifirlamaSureci
    {
        public bool Basarili { get; set; }
        public string Email { get; set; }      
        public string HataMesaji { get; set; }
    }
}
