using CeyPASS.Business.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace CeyPASS.Api.Services
{
    public class ApiEmailService : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableSsl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromAddress;
        private readonly string _fromName;
        private readonly IWebHostEnvironment _env;

        public ApiEmailService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            var smtp = configuration.GetSection("SmtpSettings");

            _host = smtp["Host"] ?? string.Empty;
            _port = int.TryParse(smtp["Port"], out var p) ? p : 587;
            _enableSsl = !bool.TryParse(smtp["EnableSsl"], out var ssl) || ssl;
            _username = smtp["Username"] ?? string.Empty;
            _password = smtp["Password"] ?? string.Empty;
            _fromAddress = smtp["FromAddress"] ?? string.Empty;
            _fromName = smtp["FromName"] ?? "CeyPASS Sistem";
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            SendInternal(toEmail, subject, body, false);
        }

        private void SendInternal(string toEmail, string subject, string body, bool isHtml)
        {
            if (string.IsNullOrWhiteSpace(_host)) return;
            try
            {
                using (var smtp = new SmtpClient(_host, _port))
                {
                    smtp.Credentials = new NetworkCredential(_username, _password);
                    smtp.EnableSsl = _enableSsl;
                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress(_fromAddress, _fromName);
                        mail.To.Add(toEmail);
                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = isHtml;
                        smtp.Send(mail);
                    }
                }
            }
            catch { /* Ignore for API demo safety */ }
        }

        public void SendVerificationCode(string toEmail, string code)
        {
            SendInternal(toEmail, "CeyPASS Doğrulama Kodu", $"Doğrulama kodunuz: {code}", false);
        }

        public string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "";
            var parts = email.Split('@');
            if (parts.Length != 2) return email;
            return $"{parts[0][0]}***@{parts[1]}";
        }
    }
}
