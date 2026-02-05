namespace CeyPASS.Business.Abstractions
{
    public interface IEmailService
    {
        void SendEmail(string toEmail, string subject, string body);
        void SendVerificationCode(string toEmail, string code);
        string MaskEmail(string email);
    }
}
