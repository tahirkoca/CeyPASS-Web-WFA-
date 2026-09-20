using CeyPASS.Business.Services;
using FluentAssertions;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// E-posta maskeleme (gizlilik) ve gönderim öncesi zorunlu alan kontrolleri.
    /// </summary>
    public class EmailServiceTests
    {
        private readonly EmailService _sut;

        public EmailServiceTests()
        {
            // Set dummy AppSettings so SmtpConfiguration.Validate() passes.
            // The SMTP server is never actually contacted in these tests.
            System.Configuration.ConfigurationManager.AppSettings["SmtpHost"] = "localhost";
            System.Configuration.ConfigurationManager.AppSettings["SmtpPort"] = "587";
            System.Configuration.ConfigurationManager.AppSettings["SmtpEnableSsl"] = "false";
            System.Configuration.ConfigurationManager.AppSettings["SmtpUsername"] = "testuser";
            System.Configuration.ConfigurationManager.AppSettings["SmtpPassword"] = "testpass";
            System.Configuration.ConfigurationManager.AppSettings["EmailFromAddress"] = "test@test.com";
            System.Configuration.ConfigurationManager.AppSettings["EmailFromName"] = "Test";

            _sut = new EmailService();
        }

        // ─── MaskEmail ────────────────────────────────────────────────────────

        /// <summary>
        /// NullVeyaboş boşStringdöner
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void MaskEmail_NullVeyaBos_BosStringDoner(string? email)
        {
            _sut.MaskEmail(email).Should().Be("");
        }

        /// <summary>
        /// AtIsaretsiz Aynendöner
        /// </summary>
        [Fact]
        public void MaskEmail_AtIsaretsiz_AynenDoner()
        {
            _sut.MaskEmail("gecersizemail").Should().Be("gecersizemail");
        }

        /// <summary>
        /// IkiKarakterKullanici Yildizlidöner
        /// </summary>
        [Fact]
        public void MaskEmail_IkiKarakterKullanici_YildizliDoner()
        {
            _sut.MaskEmail("ab@domain.com").Should().Be("**@domain.com");
        }

        /// <summary>
        /// NormalEmail MaskelenmisStringdöner
        /// </summary>
        [Fact]
        public void MaskEmail_NormalEmail_MaskelenmisStringDoner()
        {
            // "tahir" → 't' + "***" + 'r' = "t***r"
            _sut.MaskEmail("tahir@x.com").Should().Be("t***r@x.com");
        }

        // ─── SendEmail guard'ları ─────────────────────────────────────────────

        /// <summary>
        /// BoşEmail Argumentistisna fırlatılır
        /// </summary>
        [Fact]
        public void SendEmail_BosEmail_ArgumentException()
        {
            // Guard throws before any SMTP attempt
            Action act = () => _sut.SendEmail("", "Konu", "Gövde");

            act.Should().Throw<ArgumentException>().WithMessage("*boş olamaz*");
        }

        /// <summary>
        /// BoşKonu Argumentistisna fırlatılır
        /// </summary>
        [Fact]
        public void SendEmail_BosKonu_ArgumentException()
        {
            Action act = () => _sut.SendEmail("test@test.com", "   ", "Gövde");

            act.Should().Throw<ArgumentException>().WithMessage("*boş olamaz*");
        }
    }
}
