using CeyPASS.Entities.Helpers;
using FluentAssertions;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// T.C. kimlik maskeleme, doğrulama ve kayıt sırasında maskeli/tam numara çözümleme kuralları.
    /// </summary>
    public class TcKimlikHelperTests
    {
        /// <summary>
        /// 11 haneli TC gösterimde yalnızca ilk rakam açık, kalanı yıldız.
        /// </summary>
        [Fact]
        public void Mask_OnBirHane_IlkKarakterVeYildiz()
        {
            TcKimlikHelper.Mask("12345678901").Should().Be("1**********");
        }

        /// <summary>
        /// Maskeli (yıldızlı) değer geçerli TC kabul edilmez.
        /// </summary>
        [Fact]
        public void RequireValid_Yildizli_Red()
        {
            Action act = () => TcKimlikHelper.RequireValid("1**********");
            act.Should().Throw<ArgumentException>().WithMessage("*11 haneli*");
        }

        /// <summary>
        /// Formda maskeli görünüm varken kayıt için arka plandaki tam TC kullanılır.
        /// </summary>
        [Fact]
        public void ResolveForSave_MaskeliGosterim_TamTcDoner()
        {
            var kayit = TcKimlikHelper.ResolveForSave("1**********", "12345678901");
            kayit.Should().Be("12345678901");
        }

        /// <summary>
        /// Kullanıcı tam 11 hane girdiyse aynen kaydedilir.
        /// </summary>
        [Fact]
        public void ResolveForSave_ElleOnBirHane_AynenDoner()
        {
            TcKimlikHelper.ResolveForSave("12345678901", null).Should().Be("12345678901");
        }

        /// <summary>
        /// TC ve pasaport birlikte boşsa kayıt reddedilir.
        /// </summary>
        [Fact]
        public void RequireTcOrPasaport_IkisiBos_Exception()
        {
            Action act = () => TcKimlikHelper.RequireTcOrPasaport(null, "  ");
            act.Should().Throw<ArgumentException>().WithMessage("*T.C. Kimlik No veya Pasaport No*");
        }

        /// <summary>
        /// Yalnızca pasaport doluysa TC null, pasaport trimlenmiş döner.
        /// </summary>
        [Fact]
        public void RequireTcOrPasaport_SadecePasaport_Ok()
        {
            var (tc, pasaport) = TcKimlikHelper.RequireTcOrPasaport(null, " AB123 ");
            tc.Should().BeNull();
            pasaport.Should().Be("AB123");
        }

        /// <summary>
        /// Opsiyonel TC alanı boşsa kayıt için null.
        /// </summary>
        [Fact]
        public void ResolveTcOptionalForSave_Bos_Null()
        {
            TcKimlikHelper.ResolveTcOptionalForSave("  ", null).Should().BeNull();
        }
    }
}
