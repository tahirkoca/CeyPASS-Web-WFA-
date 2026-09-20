using CeyPASS.Business.Services;
using FluentAssertions;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// Fazla mesai onay ve hesaplama servisi iş kuralları.
    /// </summary>
    public class FazlaMesaiServiceTests
    {
        private readonly FazlaMesaiService _sut = new();

        // ─── Yuvarla30 ────────────────────────────────────────────────────────

        /// <summary>
        /// SıfırVeyaNegatif sıfırdöner
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Yuvarla30_SifirVeyaNegatif_SifirDoner(int dakika)
        {
            _sut.Yuvarla30(dakika).Should().Be(0);
        }

        /// <summary>
        /// 14Dakika sıfırdöner
        /// </summary>
        [Fact]
        public void Yuvarla30_14Dakika_SifirDoner()
        {
            _sut.Yuvarla30(14).Should().Be(0);
        }

        /// <summary>
        /// 15Dakika Otuzdöner
        /// </summary>
        [Fact]
        public void Yuvarla30_15Dakika_OtuzDoner()
        {
            _sut.Yuvarla30(15).Should().Be(30);
        }

        /// <summary>
        /// 30Dakika Otuzdöner
        /// </summary>
        [Fact]
        public void Yuvarla30_30Dakika_OtuzDoner()
        {
            _sut.Yuvarla30(30).Should().Be(30);
        }

        /// <summary>
        /// 45Dakika Altmishdöner
        /// </summary>
        [Fact]
        public void Yuvarla30_45Dakika_AltmishDoner()
        {
            _sut.Yuvarla30(45).Should().Be(60);
        }

        /// <summary>
        /// 60Dakika Altmishdöner
        /// </summary>
        [Fact]
        public void Yuvarla30_60Dakika_AltmishDoner()
        {
            _sut.Yuvarla30(60).Should().Be(60);
        }

        // ─── HesaplaSistemFm ──────────────────────────────────────────────────

        /// <summary>
        /// Ikisisıfır sıfırdöner
        /// </summary>
        [Fact]
        public void HesaplaSistemFm_IkisiSifir_SifirDoner()
        {
            _sut.HesaplaSistemFm(0, 0).Should().Be(0);
        }

        /// <summary>
        /// ErkenNegatif sıfırOlarakKabulEdilir
        /// </summary>
        [Fact]
        public void HesaplaSistemFm_ErkenNegatif_SifirOlarakKabulEdilir()
        {
            _sut.HesaplaSistemFm(-10, 30).Should().Be(30);
        }

        /// <summary>
        /// GecNegatif sıfırOlarakKabulEdilir
        /// </summary>
        [Fact]
        public void HesaplaSistemFm_GecNegatif_SifirOlarakKabulEdilir()
        {
            _sut.HesaplaSistemFm(30, -10).Should().Be(30);
        }

        /// <summary>
        /// Ikisi30 60döner
        /// </summary>
        [Fact]
        public void HesaplaSistemFm_Ikisi30_60Doner()
        {
            _sut.HesaplaSistemFm(30, 30).Should().Be(60);
        }

        /// <summary>
        /// 14Ve14 YuvarlamaSonrasisıfırdöner
        /// </summary>
        [Fact]
        public void HesaplaSistemFm_14Ve14_YuvarlamaSonrasiSifirDoner()
        {
            // Her ikisi de 14 → Yuvarla30(14) = 0 + Yuvarla30(14) = 0
            _sut.HesaplaSistemFm(14, 14).Should().Be(0);
        }
    }
}
