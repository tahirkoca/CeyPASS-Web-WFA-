using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CeyPASS.Business.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IMailService _mailService;
        private readonly IMailRepository _mailRepo;
        private Dictionary<string, List<string>> _aliciGruplari = new();

        public NotificationService(IMailService mailService, IMailRepository mailRepo)
        {
            _mailService = mailService;
            _mailRepo = mailRepo;
            AliciGruplariniYukle();
        }
        public List<string> AliciGrupGetir(string grupAdi)
        {
            if (_aliciGruplari.ContainsKey(grupAdi))
            {
                return _aliciGruplari[grupAdi];
            }
            return new List<string>();
        }
        public async Task<bool> GuncellemeNotifikasyonuGonderAsync(GuncellemeNotifikasyonDTO guncellemeInfo, string? logoBase64 = null)
        {
            var htmlIcerik = GuncellemeEmailHtmlOlustur(guncellemeInfo, logoBase64);
            var konu = $"CeyPASS Güncelleme - Versiyon {guncellemeInfo.VersiyonNumarasi}";

            var alicilar = new List<string>();

            if (guncellemeInfo.GuncellemeTipi == "Major" ||
                (guncellemeInfo.KritikDegisiklikler != null && guncellemeInfo.KritikDegisiklikler.Any()))
            {
                alicilar.AddRange(AliciGrupGetir("TumKullanicilar"));
                alicilar.AddRange(AliciGrupGetir("Yoneticiler"));
                alicilar.AddRange(AliciGrupGetir("UstYonetim"));
            }
            else
            {
                alicilar.AddRange(AliciGrupGetir("Yoneticiler"));
            }

            alicilar = alicilar.Distinct().ToList();

            return await _mailService.SendEmailAsync(alicilar, konu, htmlIcerik, true);
        }
        public string OnizlemeHtmlOlustur(GuncellemeNotifikasyonDTO guncellemeInfo, string? logoBase64 = null)
        {
            return GuncellemeEmailHtmlOlustur(guncellemeInfo, logoBase64);
        }
        public async Task<bool> OzelNotifikasyonGonderAsync(List<string> alicilar, string konu, string mesaj)
        {
            return await _mailService.SendEmailAsync(alicilar, konu, mesaj, true);
        }
        private string LogoBase64Al()
        {
            try
            {
                var infraAssembly = Assembly.Load("CeyPASS.WFA");
                var resourceNames = infraAssembly.GetManifestResourceNames();

                foreach (var name in resourceNames)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {name}");
                }

                var logoResourceName = resourceNames.FirstOrDefault(r =>
                    r.EndsWith("logo.png", StringComparison.OrdinalIgnoreCase) ||
                    r.EndsWith("CeyPASS 200.png", StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(logoResourceName))
                {
                    using (var stream = infraAssembly.GetManifestResourceStream(logoResourceName))
                    {
                        if (stream != null)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                stream.CopyTo(memoryStream);
                                byte[] imageBytes = memoryStream.ToArray();
                                string base64 = Convert.ToBase64String(imageBytes);
                                System.Diagnostics.Debug.WriteLine($"✓ Logo yüklendi! Boyut: {base64.Length} karakter");
                                return base64;
                            }
                        }
                    }
                }

                string[] paths = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.png"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "CeyPass 200.png")
                };

                foreach (var path in paths)
                {
                    if (File.Exists(path))
                    {
                        byte[] imageBytes = File.ReadAllBytes(path);
                        return Convert.ToBase64String(imageBytes);
                    }
                }

                System.Diagnostics.Debug.WriteLine("✗ Logo bulunamadı!");
                return string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Logo yükleme hatası: {ex.Message}");
                return string.Empty;
            }
        }
        private string GuncellemeEmailHtmlOlustur(GuncellemeNotifikasyonDTO guncellemeInfo, string? logoBase64 = null)
        {
            if (string.IsNullOrEmpty(logoBase64))
                logoBase64 = LogoBase64Al();
            var sb = new System.Text.StringBuilder();

            // Şifremi Unuttum maili ile aynı modern tasarım: gradient başlık, beyaz kart, gri palet, yuvarlak köşeler
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"tr\">");
            sb.AppendLine("<head><meta charset=\"utf-8\" /><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" /><title>CeyPASS Güncelleme</title></head>");
            sb.AppendLine("<body style=\"margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f1f5f9; -webkit-font-smoothing: antialiased;\">");
            sb.AppendLine("<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"background-color: #f1f5f9; padding: 40px 20px;\">");
            sb.AppendLine("  <tr><td align=\"center\">");
            sb.AppendLine("  <table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"max-width: 700px; margin: 0 auto; background: #ffffff; border-radius: 16px; box-shadow: 0 4px 24px rgba(0,0,0,0.08); overflow: hidden;\">");

            // HEADER – Şifremi Unuttum ile aynı kırmızı gradient
            sb.AppendLine("    <tr>");
            sb.AppendLine("      <td style=\"background: linear-gradient(135deg, #dc2626 0%, #b91c1c 50%, #991b1b 100%); padding: 28px 32px; text-align: center;\">");
            if (!string.IsNullOrEmpty(logoBase64))
            {
                sb.AppendLine($"        <img src=\"data:image/png;base64,{logoBase64}\" alt=\"CeyPASS Logo\" width=\"200\" height=\"auto\" style=\"display: block; max-width: 200px; height: auto; margin: 0 auto 16px;\" />");
            }
            else
            {
                sb.AppendLine("        <span style=\"font-size: 22px; font-weight: 700; color: #ffffff; letter-spacing: -0.5px;\">CeyPASS</span>");
                sb.AppendLine("        <p style=\"margin: 4px 0 0; font-size: 14px; color: rgba(255,255,255,0.9);\">PDKS</p>");
            }
            sb.AppendLine("        <p style=\"margin: 16px 0 8px; font-size: 20px; font-weight: 600; color: #ffffff;\">Sistem Güncellemesi</p>");
            sb.AppendLine($"        <div style=\"display: inline-block; padding: 8px 20px; background: rgba(255,255,255,0.2); border-radius: 25px; font-size: 15px; color: #ffffff; border: 1px solid rgba(255,255,255,0.3);\">Versiyon {guncellemeInfo.VersiyonNumarasi}</div>");
            sb.AppendLine($"        <p style=\"margin: 12px 0 0; font-size: 13px; color: rgba(255,255,255,0.9);\">{guncellemeInfo.YayinTarihi:dd MMMM yyyy}</p>");
            sb.AppendLine("      </td>");
            sb.AppendLine("    </tr>");

            // CONTENT – Gri metin paleti (#334155, #475569, #64748b)
            sb.AppendLine("    <tr>");
            sb.AppendLine("      <td style=\"padding: 36px 32px 32px;\">");

            if (guncellemeInfo.KritikDegisiklikler != null && guncellemeInfo.KritikDegisiklikler.Any())
            {
                sb.AppendLine("        <table width=\"100%\" cellpadding=\"16\" cellspacing=\"0\" style=\"background-color: #fef2f2; border-left: 4px solid #dc2626; border-radius: 8px; margin-bottom: 24px;\">");
                sb.AppendLine("          <tr><td>");
                sb.AppendLine("            <strong style=\"color: #b91c1c; font-size: 15px;\">Önemli bildirim</strong>");
                sb.AppendLine("            <p style=\"margin: 8px 0 0; color: #475569; font-size: 14px; line-height: 1.5;\">Bu güncelleme kritik değişiklikler içermektedir.</p>");
                sb.AppendLine("          </td></tr></table>");
                sb.AppendLine("        <h3 style=\"color: #334155; font-size: 18px; margin: 0 0 12px; font-weight: 600;\">Kritik değişiklikler</h3>");
                sb.AppendLine("        <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin-bottom: 24px;\">");
                foreach (var degisiklik in guncellemeInfo.KritikDegisiklikler)
                {
                    sb.AppendLine("          <tr><td style=\"padding: 8px 0; color: #475569; font-size: 15px; line-height: 1.6;\">• " + WebUtility.HtmlEncode(degisiklik ?? "") + "</td></tr>");
                }
                sb.AppendLine("        </table>");
                sb.AppendLine("        <hr style=\"border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;\" />");
            }

            if (guncellemeInfo.YeniOzellikler != null && guncellemeInfo.YeniOzellikler.Any())
            {
                sb.AppendLine("        <h3 style=\"color: #334155; font-size: 18px; margin: 0 0 12px; font-weight: 600;\">Yeni özellikler</h3>");
                sb.AppendLine("        <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin-bottom: 24px;\">");
                foreach (var ozellik in guncellemeInfo.YeniOzellikler)
                {
                    sb.AppendLine("          <tr><td style=\"padding: 8px 0; color: #475569; font-size: 15px; line-height: 1.6;\">• " + WebUtility.HtmlEncode(ozellik ?? "") + "</td></tr>");
                }
                sb.AppendLine("        </table>");
                sb.AppendLine("        <hr style=\"border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;\" />");
            }

            if (guncellemeInfo.Iyilestirmeler != null && guncellemeInfo.Iyilestirmeler.Any())
            {
                sb.AppendLine("        <h3 style=\"color: #334155; font-size: 18px; margin: 0 0 12px; font-weight: 600;\">İyileştirmeler</h3>");
                sb.AppendLine("        <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin-bottom: 24px;\">");
                foreach (var iyilestirme in guncellemeInfo.Iyilestirmeler)
                {
                    sb.AppendLine("          <tr><td style=\"padding: 8px 0; color: #475569; font-size: 15px; line-height: 1.6;\">• " + WebUtility.HtmlEncode(iyilestirme ?? "") + "</td></tr>");
                }
                sb.AppendLine("        </table>");
                sb.AppendLine("        <hr style=\"border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;\" />");
            }

            if (guncellemeInfo.HataDuzeltmeleri != null && guncellemeInfo.HataDuzeltmeleri.Any())
            {
                sb.AppendLine("        <h3 style=\"color: #334155; font-size: 18px; margin: 0 0 12px; font-weight: 600;\">Hata düzeltmeleri</h3>");
                sb.AppendLine("        <table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" style=\"margin-bottom: 24px;\">");
                foreach (var hata in guncellemeInfo.HataDuzeltmeleri)
                {
                    sb.AppendLine("          <tr><td style=\"padding: 8px 0; color: #475569; font-size: 15px; line-height: 1.6;\">• " + WebUtility.HtmlEncode(hata ?? "") + "</td></tr>");
                }
                sb.AppendLine("        </table>");
                sb.AppendLine("        <hr style=\"border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;\" />");
            }

            if (!string.IsNullOrWhiteSpace(guncellemeInfo.EkNotlar))
            {
                sb.AppendLine("        <h3 style=\"color: #334155; font-size: 18px; margin: 0 0 12px; font-weight: 600;\">Önemli notlar</h3>");
                sb.AppendLine("        <table width=\"100%\" cellpadding=\"16\" cellspacing=\"0\" style=\"background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; margin-bottom: 0;\">");
                sb.AppendLine("          <tr><td style=\"color: #475569; font-size: 15px; line-height: 1.6;\">" + WebUtility.HtmlEncode(guncellemeInfo.EkNotlar.Trim()) + "</td></tr>");
                sb.AppendLine("        </table>");
            }

            sb.AppendLine("      </td>");
            sb.AppendLine("    </tr>");

            // FOOTER – Şifremi Unuttum ile aynı: açık gri, ince üst çizgi
            sb.AppendLine("    <tr>");
            sb.AppendLine("      <td style=\"padding: 20px 32px 28px; border-top: 1px solid #e2e8f0; background: #f8fafc; text-align: center;\">");
            sb.AppendLine($"        <p style=\"margin: 0; font-size: 12px; color: #94a3b8;\">© {DateTime.Now.Year} CeyPASS · Bu e-posta otomatik olarak gönderilmiştir.</p>");
            sb.AppendLine("      </td>");
            sb.AppendLine("    </tr>");

            sb.AppendLine("  </table>");
            sb.AppendLine("  </td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
        private void AliciGruplariniYukle()
        {
            if (_mailRepo != null)
            {
                try
                {
                    _aliciGruplari = _mailRepo.AliciGruplariniGetir();
                    if (_aliciGruplari != null && _aliciGruplari.Any())
                    {
                        return;
                    }
                }
                catch
                {
                    // Repo hata verirse varsayılan alıcı listesi kullanılır
                }
            }
            _aliciGruplari = GetDefaultAliciGruplari();
        }
        private Dictionary<string, List<string>> GetDefaultAliciGruplari()
        {
            return new Dictionary<string, List<string>>
            {
                ["TumKullanicilar"] = new List<string>
                {
                    "tahirkoca@ceyholding.com.tr"
                }
            };
        }
    }
}
