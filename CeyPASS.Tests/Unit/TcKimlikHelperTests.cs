using CeyPASS.Entities.Helpers;
using FluentAssertions;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class TcKimlikHelperTests
    {
        [Fact]
        public void Mask_OnBirHane_IlkKarakterVeYildiz()
        {
            TcKimlikHelper.Mask("12345678901").Should().Be("1**********");
        }

        [Fact]
        public void RequireValid_Yildizli_Red()
        {
            Action act = () => TcKimlikHelper.RequireValid("1**********");
            act.Should().Throw<ArgumentException>().WithMessage("*11 haneli*");
        }

        [Fact]
        public void ResolveForSave_MaskeliGosterim_TamTcDoner()
        {
            var kayit = TcKimlikHelper.ResolveForSave("1**********", "12345678901");
            kayit.Should().Be("12345678901");
        }

        [Fact]
        public void ResolveForSave_ElleOnBirHane_AynenDoner()
        {
            TcKimlikHelper.ResolveForSave("12345678901", null).Should().Be("12345678901");
        }
    }
}
