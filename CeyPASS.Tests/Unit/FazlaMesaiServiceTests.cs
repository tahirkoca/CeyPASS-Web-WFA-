using CeyPASS.Business.Services;
using FluentAssertions;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class FazlaMesaiServiceTests
    {
        private readonly FazlaMesaiService _sut = new();

        // ─── Yuvarla30 ────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Yuvarla30_SifirVeyaNegatif_SifirDoner(int dakika)
        {
            _sut.Yuvarla30(dakika).Should().Be(0);
        }

        [Fact]
        public void Yuvarla30_14Dakika_SifirDoner()
        {
            _sut.Yuvarla30(14).Should().Be(0);
        }

        [Fact]
        public void Yuvarla30_15Dakika_OtuzDoner()
        {
            _sut.Yuvarla30(15).Should().Be(30);
        }

        [Fact]
        public void Yuvarla30_30Dakika_OtuzDoner()
        {
            _sut.Yuvarla30(30).Should().Be(30);
        }

        [Fact]
        public void Yuvarla30_45Dakika_AltmishDoner()
        {
            _sut.Yuvarla30(45).Should().Be(60);
        }

        [Fact]
        public void Yuvarla30_60Dakika_AltmishDoner()
        {
            _sut.Yuvarla30(60).Should().Be(60);
        }

        // ─── HesaplaSistemFm ──────────────────────────────────────────────────

        [Fact]
        public void HesaplaSistemFm_IkisiSifir_SifirDoner()
        {
            _sut.HesaplaSistemFm(0, 0).Should().Be(0);
        }

        [Fact]
        public void HesaplaSistemFm_ErkenNegatif_SifirOlarakKabulEdilir()
        {
            _sut.HesaplaSistemFm(-10, 30).Should().Be(30);
        }

        [Fact]
        public void HesaplaSistemFm_GecNegatif_SifirOlarakKabulEdilir()
        {
            _sut.HesaplaSistemFm(30, -10).Should().Be(30);
        }

        [Fact]
        public void HesaplaSistemFm_Ikisi30_60Doner()
        {
            _sut.HesaplaSistemFm(30, 30).Should().Be(60);
        }

        [Fact]
        public void HesaplaSistemFm_14Ve14_YuvarlamaSonrasiSifirDoner()
        {
            // Her ikisi de 14 → Yuvarla30(14) = 0 + Yuvarla30(14) = 0
            _sut.HesaplaSistemFm(14, 14).Should().Be(0);
        }
    }
}
