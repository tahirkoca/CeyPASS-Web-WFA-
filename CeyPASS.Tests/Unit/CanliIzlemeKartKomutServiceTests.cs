using CeyPASS.Business.Services;
using CeyPASS.DataAccess.Abstractions;
using CeyPASS.Entities.Concrete;
using FluentAssertions;
using Moq;
using System;
using Xunit;

namespace CeyPASS.Tests.Unit
{
    /// <summary>
    /// Kart kısıtlama kuyruğu: HAZIR kart kısıtlanamaz, kısıt varsa kaldırılır.
    /// </summary>
    public class CanliIzlemeKartKomutServiceTests
    {
        private readonly Mock<ICanliIzlemeKartKomutRepository> _repoMock = new();
        private readonly Mock<IPuantajsizKartAtamaRepository> _atamaRepoMock = new();
        private readonly CanliIzlemeKartKomutService _sut;

        public CanliIzlemeKartKomutServiceTests()
        {
            _repoMock.Setup(r => r.GetKartNoByPersonelId(It.IsAny<string>())).Returns("K1");
            _sut = new CanliIzlemeKartKomutService(_repoMock.Object, _atamaRepoMock.Object);
        }

        [Fact]
        public void EnqueuePasif_HazirKart_Exception()
        {
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(false);

            Action act = () => _sut.EnqueuePasif(1, "KART001", 9);

            act.Should().Throw<InvalidOperationException>().WithMessage("*HAZIR*");
            _repoMock.Verify(r => r.Enqueue(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public void EnqueuePasif_AtanmisKart_PasifYazilir()
        {
            _atamaRepoMock.Setup(a => a.ExistsActiveForCard("KART001")).Returns(true);

            _sut.EnqueuePasif(1, " KART001 ", 9);

            _repoMock.Verify(r => r.Enqueue(1, "KART001", "K1", CanliIzlemeKartKomutTurleri.Pasif, 9), Times.Once);
        }

        [Fact]
        public void KisitVarsaKaldir_Kisitli_AktifYazilirTrueDoner()
        {
            _repoMock.Setup(r => r.GetSonKomut(1, "KART001")).Returns(CanliIzlemeKartKomutTurleri.Pasif);

            var sonuc = _sut.KisitVarsaKaldir(1, "KART001", 9);

            sonuc.Should().BeTrue();
            _repoMock.Verify(r => r.Enqueue(1, "KART001", "K1", CanliIzlemeKartKomutTurleri.Aktif, 9), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(CanliIzlemeKartKomutTurleri.Aktif)]
        public void KisitVarsaKaldir_Serbest_KomutYazilmaz(string sonKomut)
        {
            _repoMock.Setup(r => r.GetSonKomut(1, "KART001")).Returns(sonKomut);

            var sonuc = _sut.KisitVarsaKaldir(1, "KART001", 9);

            sonuc.Should().BeFalse();
            _repoMock.Verify(r => r.Enqueue(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        }
    }
}
