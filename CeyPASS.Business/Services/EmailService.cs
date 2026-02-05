using CeyPASS.Business.Abstractions;
using CeyPASS.Infrastructure.Configuration;
using System;
using System.Net;
using System.Net.Mail;

namespace CeyPASS.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpConfiguration _config;

        public EmailService(SmtpConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _config.Validate();
        }
        public EmailService() : this(new SmtpConfiguration()) { }
        public void SendEmail(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Email adresi boş olamaz", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Email konusu boş olamaz", nameof(subject));

            try
            {
                using (var smtp = new SmtpClient(_config.Host, _config.Port))
                {
                    smtp.Credentials = new NetworkCredential(_config.Username, _config.Password);
                    smtp.EnableSsl = _config.EnableSsl;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Timeout = 30000;

                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress(_config.FromAddress, _config.FromName);
                        mail.To.Add(toEmail);
                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = false;

                        smtp.Send(mail);
                    }
                }
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException($"Email gönderilemedi: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Email gönderme sırasında beklenmeyen hata: {ex.Message}", ex);
            }
        }
        public void SendVerificationCode(string toEmail, string code)
        {
            string subject = "CeyPASS Doğrulama Kodu";
            string body = $@"Merhaba,

Şifre sıfırlama kodunuz: {code}

Bu kod 10 dakika geçerlidir.

Eğer bu isteği siz yapmadıysanız, lütfen bu e-postayı görmezden gelin.

CeyPASS Sistem";

            SendEmail(toEmail, subject, body);
        }
        public string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "";

            var parts = email.Split('@');
            if (parts.Length != 2)
                return email;

            var username = parts[0];
            var domain = parts[1];

            if (username.Length <= 2)
                return $"**@{domain}";

            return $"{username[0]}***{username[username.Length - 1]}@{domain}";
        }
    }
}
