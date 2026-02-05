using CeyPASS.Business.Abstractions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CeyPASS.Business.Services
{
    public class MailService : IMailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;
        private readonly string _senderName;
        private readonly bool _enableSsl;

        public MailService(IConfiguration configuration)
        {
            var section = configuration?.GetSection("SmtpSettings");
            _smtpHost = section?["Host"] ?? ConfigurationManager.AppSettings["SmtpHost"] ?? "";
            _smtpPort = int.TryParse(section?["Port"], out var port) ? port : (int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out port) ? port : 587);
            _senderEmail = section?["FromAddress"] ?? section?["Username"] ?? ConfigurationManager.AppSettings["EmailFromAddress"] ?? "";
            _senderPassword = section?["Password"] ?? ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
            _senderName = section?["FromName"] ?? ConfigurationManager.AppSettings["EmailFromName"] ?? "CeyPASS";
            _enableSsl = bool.TryParse(section?["EnableSsl"], out var ssl) ? ssl : bool.TryParse(ConfigurationManager.AppSettings["SmtpEnableSsl"], out ssl) && ssl;
        }

        public MailService()
        {
            _smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "";
            _smtpPort = int.TryParse(ConfigurationManager.AppSettings["SmtpPort"], out var port) ? port : 587;
            _senderEmail = ConfigurationManager.AppSettings["EmailFromAddress"] ?? "";
            _senderPassword = ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
            _senderName = ConfigurationManager.AppSettings["EmailFromName"] ?? "CeyPASS";
            _enableSsl = bool.TryParse(ConfigurationManager.AppSettings["SmtpEnableSsl"], out var ssl) ? ssl : true;
        }
        public async Task<bool> SendEmailAsync(List<string> alicilar, string konu, string icerik, bool htmlMi = true)
        {
            try
            {
                using (var client = new SmtpClient(_smtpHost, _smtpPort))
                {
                    client.EnableSsl = _enableSsl;
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_senderEmail, _senderName),
                        Subject = konu,
                        Body = icerik,
                        IsBodyHtml = htmlMi
                    };

                    foreach (var alici in alicilar)
                    {
                        mailMessage.To.Add(alici);
                    }

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Email gönderme hatası: {ex.Message}");
                return false;
            }
        }
    }
}
