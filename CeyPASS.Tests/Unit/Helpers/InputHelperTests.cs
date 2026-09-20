using CeyPASS.Infrastructure.Helpers;
using FluentAssertions;
using Xunit;

namespace CeyPASS.Tests.Unit.Helpers
{
    /// <summary>
    /// Form girdisi: yıl parse aralığı (1900–2100) ve virgüllü ID listesi ayrıştırma.
    /// </summary>
    public class InputHelperTests
    {
        // ─── TryParseYear ─────────────────────────────────────────────────────

        /// <summary>
        /// GeçerliYil Truedöner
        /// </summary>
        [Fact]
        public void TryParseYear_GecerliYil_TrueDoner()
        {
            var result = InputHelper.TryParseYear("2024", out int year);
            result.Should().BeTrue();
            year.Should().Be(2024);
        }

        /// <summary>
        /// SinirAlt1900 Truedöner
        /// </summary>
        [Fact]
        public void TryParseYear_SinirAlt1900_TrueDoner()
        {
            var result = InputHelper.TryParseYear("1900", out int year);
            result.Should().BeTrue();
            year.Should().Be(1900);
        }

        /// <summary>
        /// SinirUst2100 Truedöner
        /// </summary>
        [Fact]
        public void TryParseYear_SinirUst2100_TrueDoner()
        {
            var result = InputHelper.TryParseYear("2100", out int year);
            result.Should().BeTrue();
            year.Should().Be(2100);
        }

        /// <summary>
        /// 1899 Falsedöner
        /// </summary>
        [Fact]
        public void TryParseYear_1899_FalseDoner()
        {
            var result = InputHelper.TryParseYear("1899", out int year);
            result.Should().BeFalse();
        }

        /// <summary>
        /// SayisalOlmayan Falsedöner
        /// </summary>
        [Fact]
        public void TryParseYear_SayisalOlmayan_FalseDoner()
        {
            var result = InputHelper.TryParseYear("abc", out int year);
            result.Should().BeFalse();
        }

        // ─── ParseCsvIds ──────────────────────────────────────────────────────

        /// <summary>
        /// BoşString boşSetdöner
        /// </summary>
        [Fact]
        public void ParseCsvIds_BosString_BosSetDoner()
        {
            var result = InputHelper.ParseCsvIds("");
            result.Should().BeEmpty();
        }

        /// <summary>
        /// TekGeçerliId TekElemandöner
        /// </summary>
        [Fact]
        public void ParseCsvIds_TekGecerliId_TekElemanDoner()
        {
            var result = InputHelper.ParseCsvIds("5");
            result.Should().ContainSingle().Which.Should().Be(5);
        }

        /// <summary>
        /// MukerrerIdler BirKezSayilir
        /// </summary>
        [Fact]
        public void ParseCsvIds_MukerrerIdler_BirKezSayilir()
        {
            var result = InputHelper.ParseCsvIds("3,3,3");
            result.Should().HaveCount(1).And.Contain(3);
        }

        /// <summary>
        /// GeçersizVeGeçerliKarisik SadaceGeçerliler
        /// </summary>
        [Fact]
        public void ParseCsvIds_GecersizVeGecerliKarisik_SadaceGecerliler()
        {
            var result = InputHelper.ParseCsvIds("1,abc,2");
            result.Should().HaveCount(2).And.Contain(new[] { 1, 2 });
        }
    }
}
