namespace CeyPASS.Business.Abstractions
{
    /// <summary>SMTP tabanlı e-posta gönderimi ve maskeleme.</summary>
    public interface IEmailService
    {
        /// <summary>Genel e-posta gönderir.</summary>
        void SendEmail(string toEmail, string subject, string body);

        /// <summary>Doğrulama kodu e-postası gönderir.</summary>
        void SendVerificationCode(string toEmail, string code);

        /// <summary>E-posta adresini güvenli gösterim için maskeler.</summary>
        string MaskEmail(string email);
    }
}
