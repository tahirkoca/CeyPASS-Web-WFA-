using CeyPASS.Business.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace CeyPASS.Web.Services
{
    /// <summary>
    /// ASP.NET Core için IConfiguration'dan SMTP ayarlarını okuyan EmailService
    /// </summary>
    public class EmailServiceCore : IEmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableSsl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _fromAddress;
        private readonly string _fromName;
        private readonly IWebHostEnvironment _env;

        public EmailServiceCore(IConfiguration configuration, IWebHostEnvironment env)
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
            SendEmailInternal(toEmail, subject, body, isBodyHtml: false);
        }

        private void SendEmailInternal(string toEmail, string subject, string body, bool isBodyHtml)
        {
            if (string.IsNullOrWhiteSpace(_host))
                throw new InvalidOperationException("SMTP ayarları yapılandırılmamış. appsettings.json'da SmtpSettings bölümünü kontrol edin.");

            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Email adresi boş olamaz", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Email konusu boş olamaz", nameof(subject));

            try
            {
                using (var smtp = new SmtpClient(_host, _port))
                {
                    smtp.Credentials = new NetworkCredential(_username, _password);
                    smtp.EnableSsl = _enableSsl;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Timeout = 30000;

                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress(_fromAddress, _fromName);
                        mail.To.Add(toEmail);
                        mail.Subject = subject;
                        mail.Body = body;
                        mail.IsBodyHtml = isBodyHtml;
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
            string logoBase64 = GetLogoBase64();
            string htmlBody = BuildVerificationCodeEmailHtml(code, logoBase64);
            SendEmailInternal(toEmail, subject, htmlBody, isBodyHtml: true);
        }

        /// <summary>
        /// wwwroot/images/logo.png dosyasını base64 olarak okur (e-postada gömülü logo için).
        /// </summary>
        private string GetLogoBase64()
        {
            try
            {
                string basePath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath ?? "", "wwwroot");
                string path = Path.Combine(basePath, "images", "logo.png");
                if (!File.Exists(path))
                    return null;
                byte[] bytes = File.ReadAllBytes(path);
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Şifre sıfırlama doğrulama kodu e-postası için modern HTML şablonu (e-posta istemcileriyle uyumlu, inline stiller).
        /// </summary>
        private static string BuildVerificationCodeEmailHtml(string code, string logoBase64)
        {
            string codeEscaped = System.Net.WebUtility.HtmlEncode(code ?? "");
            string logoHtml = !string.IsNullOrEmpty(logoBase64)
                ? $"<img src=\"data:image/png;base64,{logoBase64}\" alt=\"CeyPASS\" width=\"180\" height=\"auto\" style=\"display:block; max-width:180px; height:auto; margin:0 auto 12px;\" />"
                : "<span style=\"font-size:22px; font-weight:700; color:#ffffff; letter-spacing:-0.5px;\">CeyPASS</span>";
            return $@"<!DOCTYPE html>
<html lang=""tr"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<title>CeyPASS Doğrulama Kodu</title>
</head>
<body style=""margin:0; padding:0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color:#f1f5f9; -webkit-font-smoothing:antialiased;"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f1f5f9; padding: 40px 20px;"">
<tr><td align=""center"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:520px; margin:0 auto; background:#ffffff; border-radius:16px; box-shadow: 0 4px 24px rgba(0,0,0,0.08); overflow:hidden;"">
<tr>
<td style=""background: linear-gradient(135deg, #dc2626 0%, #b91c1c 50%, #991b1b 100%); padding: 28px 32px; text-align:center;"">
{logoHtml}
<p style=""margin:6px 0 0; font-size:13px; color:rgba(255,255,255,0.9);"">Doğrulama Kodu</p>
</td>
</tr>
<tr>
<td style=""padding: 36px 32px 32px;"">
<p style=""margin:0 0 20px; font-size:16px; color:#334155; line-height:1.5;"">Merhaba,</p>
<p style=""margin:0 0 16px; font-size:15px; color:#475569; line-height:1.5;"">Şifre sıfırlama için doğrulama kodunuz:</p>
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin: 20px 0 24px;"">
<tr><td align=""center"">
<div style=""display:inline-block; padding: 18px 32px; background:#f8fafc; border:2px solid #e2e8f0; border-radius:12px; font-size:28px; font-weight:700; letter-spacing:8px; color:#1e293b; font-family: 'Consolas', 'Monaco', monospace;"">{codeEscaped}</div>
</td></tr>
</table>
<p style=""margin:0 0 8px; font-size:14px; color:#64748b; line-height:1.5;"">Bu kod <strong style=""color:#475569;"">10 dakika</strong> geçerlidir.</p>
<p style=""margin: 24px 0 0; font-size:13px; color:#94a3b8; line-height:1.5;"">Eğer bu isteği siz yapmadıysanız, bu e-postayı dikkate almayın. Hesabınız güvende kalacaktır.</p>
</td>
</tr>
<tr>
<td style=""padding: 20px 32px 28px; border-top: 1px solid #e2e8f0; background:#f8fafc; text-align:center;"">
<p style=""margin:0; font-size:12px; color:#94a3b8;"">© CeyPASS · Bu e-posta otomatik olarak gönderilmiştir.</p>
</td>
</tr>
</table>
</td></tr>
</table>
</body>
</html>";
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
