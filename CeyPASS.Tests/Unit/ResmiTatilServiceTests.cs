using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    public class ResmiTatilServiceTests
    {
        private readonly Mock<IResmiTatilRepository> _repoMock = new();
        private readonly ResmiTatilService _sut;

        public ResmiTatilServiceTests()
        {
            _sut = new ResmiTatilService(_repoMock.Object);
        }

        // ─── DoldurSabit ──────────────────────────────────────────────────────

        [Fact]
        public void DoldurSabit_BitisYiliBastandanKucuk_Exception()
        {
            Action act = () => _sut.DoldurSabit(2025, 2024);

            act.Should().Throw<ArgumentException>().WithMessage("*küçük olamaz*");
        }

        [Fact]
        public void DoldurSabit_GecerliYillar_RepoCagrilir()
        {
            _sut.DoldurSabit(2024, 2026);

            _repoMock.Verify(r => r.DoldurSabit(2024, 2026), Times.Once);
        }

        // ─── KaydetTekil ──────────────────────────────────────────────────────

        [Fact]
        public void KaydetTekil_AdBos_Exception()
        {
            Action act = () => _sut.KaydetTekil(DateTime.Today, "   ", null);

            act.Should().Throw<ArgumentException>().WithMessage("*boş olamaz*");
        }

        [Fact]
        public void KaydetTekil_NegatifCalismaSaati_Exception()
        {
            Action act = () => _sut.KaydetTekil(DateTime.Today, "Yılbaşı", -1m);

            act.Should().Throw<ArgumentException>().WithMessage("*negatif olamaz*");
        }

        [Fact]
        public void KaydetTekil_GecerliAdVeSaati_RepoCagrilir()
        {
            var tarih = new DateTime(2025, 1, 1);

            _sut.KaydetTekil(tarih, "  Yılbaşı  ", 0m);

            _repoMock.Verify(r => r.EkleVeyaGuncelle(tarih, "Yılbaşı", 0m), Times.Once);
        }
    }
}
