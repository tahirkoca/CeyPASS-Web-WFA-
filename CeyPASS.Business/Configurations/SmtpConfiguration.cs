using System.Configuration;

namespace CeyPASS.Infrastructure.Configuration
{
    public class SmtpConfiguration
    {
        public string Host { get; }
        public int Port { get; }
        public bool EnableSsl { get; }
        public string Username { get; }
        public string Password { get; }
        public string FromAddress { get; }
        public string FromName { get; }

        public SmtpConfiguration()
        {
            Host = ConfigurationManager.AppSettings["SmtpHost"] ?? string.Empty;
            string portStr = ConfigurationManager.AppSettings["SmtpPort"];
            Port = !string.IsNullOrEmpty(portStr) ? int.Parse(portStr) : 587;
            string sslStr = ConfigurationManager.AppSettings["SmtpEnableSsl"];
            EnableSsl = !string.IsNullOrEmpty(sslStr) ? bool.Parse(sslStr) : true;
            Username = ConfigurationManager.AppSettings["SmtpUsername"] ?? string.Empty;
            Password = ConfigurationManager.AppSettings["SmtpPassword"] ?? string.Empty;
            FromAddress = ConfigurationManager.AppSettings["EmailFromAddress"] ?? string.Empty;
            FromName = ConfigurationManager.AppSettings["EmailFromName"] ?? string.Empty;
        }
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Host))
                throw new ConfigurationErrorsException("SmtpHost ayarı bulunamadı.");

            if (string.IsNullOrWhiteSpace(Username))
                throw new ConfigurationErrorsException("SmtpUsername ayarı bulunamadı.");

            if (string.IsNullOrWhiteSpace(Password))
                throw new ConfigurationErrorsException("SmtpPassword ayarı bulunamadı.");

            if (string.IsNullOrWhiteSpace(FromAddress))
                throw new ConfigurationErrorsException("EmailFromAddress ayarı bulunamadı.");
        }
    }
}
